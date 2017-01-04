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
using SizePhotos.VisualOptimization;


namespace SizePhotos
{
    class Program
    {
        static readonly string[] PHOTO_EXTENSIONS = { ".jpg", ".nef" };

        bool _errorsEncountered;
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
            if(!Directory.Exists(_opts.LocalPhotoRoot))
            {
                throw new DirectoryNotFoundException(string.Concat("The picture directory specified, ", _opts.LocalPhotoRoot, ", does not exist.  Please specify a directory containing photos."));
            }
            
            if(File.Exists(_opts.Outfile))
            {
                throw new IOException(string.Concat("The specified output file, ", _opts.Outfile, ", already exists.  Please remove it before running this process."));
            }
            
            BuildPipeline();
            PrepareDirectories();
            ResizePhotos();

            if(_errorsEncountered)
            {
                var sep = new string('*', 50);

                Console.WriteLine(sep);
                Console.WriteLine("** Some files had errors, please review!");
                Console.WriteLine(sep);

                Environment.Exit(1);
            }
        }
        

        void BuildPipeline()
        {
            if(_opts.FastReview)
            {
                // read (try raw first)
                _pipeline.AddProcessor(new DcrawPhotoReaderPhotoProcessor(_opts.Quiet, _opts.FastReview, _pathHelper));
                _pipeline.AddProcessor(new PhotoReaderPhotoProcessor(_opts.Quiet, _pathHelper));

                // write
                _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "review", 0, 0, false, _pathHelper));

                // terminate
                _pipeline.AddProcessor(new ContextTerminatorPhotoProcessor());
            }
            else
            {
                // move source file
                _pipeline.AddProcessor(new MovePhotoProcessor(_opts.Quiet, "src", true));

                // load metadata
                _pipeline.AddProcessor(new ExifPhotoProcessor(_opts.Quiet));

                // read (try raw first)
                _pipeline.AddProcessor(new DcrawPhotoReaderPhotoProcessor(_opts.Quiet, _opts.FastReview, _pathHelper));
                _pipeline.AddProcessor(new PhotoReaderPhotoProcessor(_opts.Quiet, _pathHelper));

                // visually optimize photos
                _pipeline.AddProcessor(new OptimizationPhotoProcessor(_opts.Quiet));

                // minify
                _pipeline.AddProcessor(new JpgQualityPhotoProcessor(_opts.Quiet));

                // write
                _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "xs", 120, 160, true, _pathHelper));
                _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "sm", 480, 640, true, _pathHelper));
                _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "md", 768, 1024, true, _pathHelper));
                _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "lg", 0, 0, true, _pathHelper));
                _pipeline.AddProcessor(new PhotoWriterPhotoProcessor(_opts.Quiet, "prt", 0, 0, false, _pathHelper));

                // terminate
                _pipeline.AddProcessor(new ContextTerminatorPhotoProcessor());
            }
        }

                
        void PrepareDirectories()
        {
            var outputs = _pipeline.GetOutputProcessors();

            foreach(var output in outputs)
            {
                var dir = output.OutputSubdirectory;

                if(Directory.Exists(_pathHelper.GetScaledLocalPath(dir)))
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
            if(!_opts.Quiet)
            {
                Console.WriteLine($"Processing: {Path.GetFileName(file)}");
            }

            var result = _pipeline.ProcessPhotoAsync(file).Result;

            lock(_lockObj)
            {
                if(result.HasErrors)
                {
                    _errorsEncountered = true;

                    Console.WriteLine($"Error with file: {result.SourceFile}");
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
            if(_opts.InsertMode)
            {
                return new PgsqlInsertResultWriter(_opts.Outfile);
            }
            else if(_opts.UpdateMode)
            {
                return new PgsqlUpdateResultWriter(_opts.Outfile);
            }

            return new NoopResultWriter();
        }
    }
}
