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
        SizePhotoOptions _opts;
        IResultWriter _writer;
        ProcessingTarget SourceResizeTarget { get; set; }
        ProcessingTarget XsResizeTarget { get; set; }
        ProcessingTarget SmResizeTarget { get; set; }
        ProcessingTarget MdResizeTarget { get; set; }
        ProcessingTarget LgResizeTarget { get; set; }
        List<ProcessingTarget> ResizeTargetList { get; } = new List<ProcessingTarget>();
        
        
        public Program(string[] args)
        {
            _opts = new SizePhotoOptions(args);
            _opts.ProcessArgs(args, null);
            
            _category = new CategoryInfo {
                Name = _opts.CategoryName,
                Year = _opts.Year,
                IsPrivate = _opts.IsPrivate
            };
            
            // TODO: make the writer dynamic based on opts
            _writer = new SqlInsertWriter(_opts.Outfile);
        }
        
        
        public static void Main(string[] args)
        {
            var p = new Program(args);
            p.Run();
        }
                              
                              
        void Run()
        {
            if(!_opts.ValidateOptions())
            {
                ShowUsage(_opts);
                Environment.Exit(1);
            }
            
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
            SourceResizeTarget = GetResizeTarget("src", 0, 0, null);
            
            // scale + optimize
            XsResizeTarget = GetResizeTarget("xs", 120, 160, JPG_COMPRESSION_QUALITY);
            SmResizeTarget = GetResizeTarget("sm", 480, 640, JPG_COMPRESSION_QUALITY);
            MdResizeTarget = GetResizeTarget("md", 768, 1024, JPG_COMPRESSION_QUALITY);
            LgResizeTarget = GetResizeTarget("lg", 0, 0, JPG_COMPRESSION_QUALITY);
        }
        
        
        ProcessingTarget GetResizeTarget(string pathSegment, uint maxHeight, uint maxWidth, uint? quality)
        {
            return new ProcessingTarget {
                LocalPath = _opts.GetLocalScaledPath(pathSegment),
                WebPath = _opts.GetWebScaledPath(pathSegment),
                MaxHeight = maxHeight,
                MaxWidth = maxWidth,
                Quality = quality
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
                if(Directory.Exists(target.LocalPath))
                {
                    throw new IOException("At least one of the resize directories already exist.  Please ensure you need to run this script, and if so, remove these directories.");
                }
                else
                {
                    Directory.CreateDirectory(target.LocalPath);
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
            if(!_opts.Quiet)
            {
                Console.WriteLine("Processing: " + Path.GetFileName(file));
            }

            var proc = new PhotoProcessor(SourceResizeTarget, XsResizeTarget, SmResizeTarget, MdResizeTarget, LgResizeTarget, _opts.Quiet);
            var result = proc.ProcessPhotoAsync(file).Result;

            lock(_lockObj)
            {
                _writer.Write(result);
            }
        }
        
        
        static void ShowUsage(SizePhotoOptions options)
        {
            options.DoHelp();
        }
    }
}
