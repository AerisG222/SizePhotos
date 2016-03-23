using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMagickWand;
using SizePhotos.ResultWriters;


namespace SizePhotos
{
    class Program
    {
        const int JPG_COMPRESSION_QUALITY = 88;


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
        
        
        public Program(string[] args)
        {
            try
            {
                _opts = new SizePhotoOptions();
                _opts.ProcessArgs(args, null);
                
                var errors = _opts.ValidateOptions().ToList();
            
                if(errors.Count > 0)
                {
                    ShowUsage(_opts, errors);
                    Environment.Exit(1);
                }
            
                _pathHelper = _opts.GetPathHelper();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Sorry, there was an error: {ex.Message}");
                
                ShowUsage(_opts, null);
                Environment.Exit(1);
            }
            
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
            var p = new Program(args);
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
            
            ResizePhotos();
        }
        
        
        void PrepareResizeTargets()
        {
            // original untouched image
            SourceResizeTarget = GetResizeTarget("src", 0, 0, false, null);
            
            // scale + optimize
            XsResizeTarget = GetResizeTarget("xs", 120, 160, true, JPG_COMPRESSION_QUALITY);
            SmResizeTarget = GetResizeTarget("sm", 480, 640, true, JPG_COMPRESSION_QUALITY);
            MdResizeTarget = GetResizeTarget("md", 768, 1024, true, JPG_COMPRESSION_QUALITY);
            LgResizeTarget = GetResizeTarget("lg", 0, 0, true, JPG_COMPRESSION_QUALITY);
        }
        
        
        ProcessingTarget GetResizeTarget(string pathSegment, uint maxHeight, uint maxWidth, bool optimize, uint? quality)
        {
            return new ProcessingTarget {
                ScaledPathSegment = pathSegment,
                MaxHeight = maxHeight,
                MaxWidth = maxWidth,
                Quality = quality,
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

            var proc = new PhotoProcessor(_pathHelper, SourceResizeTarget, XsResizeTarget, SmResizeTarget, MdResizeTarget, LgResizeTarget, _opts.Quiet);
            var result = proc.ProcessPhotoAsync(file).Result;

            lock(_lockObj)
            {
                _writer.AddResult(result);
            }
        }
        
        
        static void ShowUsage(SizePhotoOptions options, IList<string> errors)
        {
            options.DoHelp();
            
            if(errors != null)
            {
                Console.WriteLine();
                Console.WriteLine("The following errors were encountered:");
            
                foreach(var err in errors)
                {
                    Console.WriteLine(err);
                }
            }
        }
    }
}
