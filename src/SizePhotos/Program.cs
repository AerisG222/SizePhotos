using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMagickWand;
using SizePhotos.ResultWriters;
using CommandLine;
using CommandLine.Text;
using SizePhotos.Optimizer;
using SizePhotos.Raw;
using SizePhotos.Exif;
using SizePhotos.Quality;

namespace SizePhotos
{
    class Program
    {
        readonly object _lockObj = new object();
        readonly CategoryInfo _category;
        readonly SizePhotoOptions _opts;
        readonly PhotoPathHelper _pathHelper;
        readonly IResultWriter _writer;
        ProcessingTarget SourceResizeTarget { get; set; }
        ProcessingTarget XsResizeTarget { get; set; }
        ProcessingTarget SmResizeTarget { get; set; }
        ProcessingTarget MdResizeTarget { get; set; }
        ProcessingTarget LgResizeTarget { get; set; }
        List<ProcessingTarget> ResizeTargetList { get; } = new List<ProcessingTarget>();
        
        
        public Program(SizePhotoOptions opts)
        {
            _opts = opts;
            _pathHelper = _opts.GetPathHelper();
            
            _category = new CategoryInfo {
                Name = _opts.CategoryName,
                Year = _opts.Year,
                IsPrivate = _opts.IsPrivate
            };
            
            if(_opts.InsertMode)
            {
                _writer = new SqlInsertWriter(_opts.Outfile);
            }
            else if(_opts.UpdateMode)
            {
                _writer = new SqlUpdateWriter(_opts.Outfile);
            }
            else
            {
                _writer = new NoopWriter();
            }
        }
        
        
        public static void Main(string[] args)
        {
            var parser = new Parser(config => config.HelpWriter = null);
            var result = parser.ParseArguments<SizePhotoOptions>(args);
            
            var exitCode = result.MapResult(
                opts => 
                {
                    var errs = opts.ValidateOptions().ToList();
                    
                    if(errs.Count > 0)
                    {
                        ShowUsage(errs);
                        return 1;
                    }
            
                    var p = new Program(opts);
                    p.Run();
                    return 0;
                },
                errors =>
                {
                    ShowUsage();
                    return 1;
                }
            );
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
            
            ResizePhotos();
        }
        
        
        void PrepareResizeTargets()
        {
            // original untouched image
            SourceResizeTarget = GetResizeTarget("src", 0, 0, false);
            
            // scale + optimize
            // smaller images are more affected by quality, so on xs we force them
            // to save at higher quality if needed
            XsResizeTarget = GetResizeTarget("xs", 120, 160, true);
            SmResizeTarget = GetResizeTarget("sm", 480, 640, true);
            MdResizeTarget = GetResizeTarget("md", 768, 1024, true);
            LgResizeTarget = GetResizeTarget("lg", 0, 0, true);
        }
        
        
        ProcessingTarget GetResizeTarget(string pathSegment, uint maxHeight, uint maxWidth, bool optimize)
        {
            return new ProcessingTarget {
                ScaledPathSegment = pathSegment,
                MaxHeight = maxHeight,
                MaxWidth = maxWidth,
                Optimize = optimize
            };
        }
        
        
        void PrepareDirectories()
        {
            var targets = new List<ProcessingTarget> { 
                SourceResizeTarget,
                XsResizeTarget,
                SmResizeTarget,
                MdResizeTarget,
                LgResizeTarget
            };
            
            foreach(var target in targets)
            {
                if(Directory.Exists(_pathHelper.GetScaledLocalPath(target.ScaledPathSegment)))
                {
                    throw new IOException("At least one of the resize directories already exist.  Please ensure you need to run this script, and if so, remove these directories.");
                }
                else
                {
                    Directory.CreateDirectory(_pathHelper.GetScaledLocalPath(target.ScaledPathSegment));
                }
            }
        }

        
        void ResizePhotos()
        {
            var files = Directory.GetFiles(_opts.LocalPhotoRoot).ToList();
            var vpus = Environment.ProcessorCount - 1;

            PrepareResizeTargets();
            PrepareDirectories();
            _writer.PreProcess(_category);
            MagickWandEnvironment.Genesis();
            
            if(vpus < 1)
            {
                vpus = 1;
            }

            // try to leave a couple threads available for the GC
            var opts = new ParallelOptions { MaxDegreeOfParallelism = vpus };

            Parallel.ForEach(files, opts, ProcessPhoto);
            
            MagickWandEnvironment.Terminus();
            _writer.PostProcess();
        }
        
        
        void ProcessPhoto(string file)
        {
            file = Path.GetFileName(file);
            
            if(!_opts.Quiet)
            {
                Console.WriteLine($"Processing: {file}");
            }

            var proc = new PhotoProcessor(_pathHelper, 
                                          new PhotoOptimizer(_opts.Quiet), 
                                          new RawConverter(_opts.Quiet), 
                                          new ExifReader(_opts.Quiet), 
                                          new QualitySearcher(_opts.Quiet),
                                          SourceResizeTarget, 
                                          XsResizeTarget, 
                                          SmResizeTarget, 
                                          MdResizeTarget, 
                                          LgResizeTarget, 
                                          _opts.Quiet);
                                          
            var result = proc.ProcessPhotoAsync(file).Result;

            lock(_lockObj)
            {
                _writer.AddResult(result);
            }
        }
        
        
        static void ShowUsage(IList<string> errors = null)
        {
            var help = new HelpText();
            
            help.Heading = "SizePhotos";
            help.AddPreOptionsLine("A tool to prepare and optimize images for display on the web.");
            
            // this is a little lame, but force a NotParsed<T> options result
            // so that we can get a nice help screen.  this might be required
            // if the passed args are valid to the parser, but not w/ custom 
            // validation logic that runs after parsing
            var parser = new Parser(config => config.HelpWriter = null);
            var result = parser.ParseArguments<SizePhotoOptions>(new string[] { "--xxx" });
            help.AddOptions(result);
            
            if(errors != null)
            {
                help.AddPostOptionsLine("Errors:");
                help.AddPostOptionsLines(errors);
            }
            
            Console.WriteLine(help.ToString());            
        }
    }
}
