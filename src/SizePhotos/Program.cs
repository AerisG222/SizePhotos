using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMagickWand;
using SizePhotos.Exif;
using SizePhotos.Minification;
using SizePhotos.PhotoReaders;
using SizePhotos.PhotoWriters;
using SizePhotos.ResultWriters;


namespace SizePhotos;

class Program
{
    static readonly string[] PHOTO_EXTENSIONS = { ".jpg", ".nef" };

    List<ProcessingContext> _errorContexts = new List<ProcessingContext>();
    readonly object _lockObj = new object();
    readonly SizePhotoOptions _opts;
    readonly PhotoPathHelper _pathHelper;
    readonly IResultWriter _writer;
    readonly PhotoProcessingPipeline _pipeline = new PhotoProcessingPipeline();


    public Program(SizePhotoOptions opts)
    {
        _opts = opts;
        _pathHelper = _opts.GetPathHelper();
        _writer = GetWriter();
    }


    public static void Main(string[] args)
    {
        var opts = new SizePhotoOptions();
        opts.Parse(args);

        var p = new Program(opts);
        p.Run();
    }


    void Run()
    {
        if (!Directory.Exists(_opts.LocalPhotoRoot))
        {
            throw new DirectoryNotFoundException(string.Concat("The picture directory specified, ", _opts.LocalPhotoRoot, ", does not exist.  Please specify a directory containing photos."));
        }

        if (File.Exists(_opts.Outfile))
        {
            throw new IOException(string.Concat("The specified output file, ", _opts.Outfile, ", already exists.  Please remove it before running this process."));
        }

        BuildPipeline();
        PrepareDirectories();
        ResizePhotos();

        if (_errorContexts.Count > 0)
        {
            var sep = new string('*', 50);

            Console.WriteLine(sep);
            Console.WriteLine("** Some files had errors, please review!");
            Console.WriteLine(sep);

            foreach (var ctx in _errorContexts)
            {
                Console.WriteLine(ctx.SourceFile);

                foreach (var msg in ctx.ErrorMessages)
                {
                    Console.WriteLine($"  - {msg}");
                }
            }

            Environment.Exit(1);
        }
    }


    void BuildPipeline()
    {
        if (_opts.FastReview)
        {
            // read
            _pipeline.AddProcessor(new RawTherapeePhotoReaderPhotoProcessor(_opts.Quiet, _pathHelper));

            // write
            _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "review", _pathHelper));

            // terminate
            _pipeline.AddProcessor(new ContextTerminatorPhotoProcessor());
        }
        else
        {
            // move source file
            _pipeline.AddProcessor(new MovePhotoProcessor(_opts.Quiet, "src", true));

            // load metadata
            _pipeline.AddProcessor(new ExifPhotoProcessor());

            // read
            _pipeline.AddProcessor(new RawTherapeePhotoReaderPhotoProcessor(_opts.Quiet, _pathHelper));

            // strip metadata
            _pipeline.AddProcessor(new StripMetadataPhotoProcessor());

            // write
            _pipeline.AddProcessor(new PhotoWriterFixedSizePhotoProcessor(_opts.Quiet, "xs_sq", 120, 160, _pathHelper));
            _pipeline.AddProcessor(new PhotoWriterScalePhotoProcessor(_opts.Quiet, "xs", 120, 160, _pathHelper));
            _pipeline.AddProcessor(new PhotoWriterScalePhotoProcessor(_opts.Quiet, "sm", 480, 640, _pathHelper));
            _pipeline.AddProcessor(new PhotoWriterScalePhotoProcessor(_opts.Quiet, "md", 768, 1024, _pathHelper));
            _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "lg", _pathHelper));
            _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "prt", _pathHelper));

            // minify
            _pipeline.AddProcessor(new MinifyPhotoProcessor("xs", 72, _pathHelper));
            _pipeline.AddProcessor(new MinifyPhotoProcessor("sm", 72, _pathHelper));
            _pipeline.AddProcessor(new MinifyPhotoProcessor("md", 72, _pathHelper));
            _pipeline.AddProcessor(new MinifyPhotoProcessor("lg", 72, _pathHelper));

            // terminate
            _pipeline.AddProcessor(new ContextTerminatorPhotoProcessor());
        }
    }


    void PrepareDirectories()
    {
        var outputs = _pipeline.GetOutputProcessors();

        foreach (var output in outputs)
        {
            var dir = output.OutputSubdirectory;

            if (Directory.Exists(_pathHelper.GetScaledLocalPath(dir)))
            {
                throw new IOException("At least one of the resize directories already exist.  Please ensure you need to run this script, and if so, remove these directories.");
            }
            else
            {
                Directory.CreateDirectory(_pathHelper.GetScaledLocalPath(dir));
            }
        }
    }


    void ResizePhotos()
    {
        var files = GetPhotos();
        var vpus = Math.Max(Environment.ProcessorCount - 1, 1);

        _writer.PreProcess(_opts.CategoryInfo);

        MagickWandEnvironment.Genesis();

        // try to leave a couple threads available for the GC
        var opts = new ParallelOptions { MaxDegreeOfParallelism = vpus };

        Parallel.ForEach(files, opts, ProcessPhoto);

        MagickWandEnvironment.Terminus();
        _writer.PostProcess();
    }


    void ProcessPhoto(string file)
    {
        if (!_opts.Quiet)
        {
            Console.WriteLine($"Processing: {Path.GetFileName(file)}");
        }

        var result = _pipeline.ProcessPhotoAsync(file).Result;

        // additional check to clean up any wands - in particular for errors
        if (result.Wand != null)
        {
            result.Wand.Dispose();
        }

        lock (_lockObj)
        {
            if (result.HasErrors)
            {
                _errorContexts.Add(result);
            }
            else
            {
                _writer.AddResult(result);
            }
        }
    }


    IEnumerable<string> GetPhotos()
    {
        return Directory.GetFiles(_opts.LocalPhotoRoot)
            .Where(x => PHOTO_EXTENSIONS.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
            .OrderBy(x => x)
            .ToList();
    }


    IResultWriter GetWriter()
    {
        if (_opts.InsertMode)
        {
            return new PgsqlInsertResultWriter(_opts.Outfile);
        }
        else if (_opts.UpdateMode)
        {
            return new PgsqlUpdateResultWriter(_opts.Outfile);
        }

        return new NoopResultWriter();
    }
}
