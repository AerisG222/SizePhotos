using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMagickWand;


namespace SizePhotos
{
    class Program
    {
        const int JPG_COMPRESSION_QUALITY = 88;


        readonly object _lockObj = new object();
        StreamWriter _writer;
        bool _hasSetTeaserPhoto;
        SizePhotoOptions _opts;
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
            PrepareOutputStream();
            MagickWandEnvironment.Genesis();
            
            if(vpus < 1)
            {
                vpus = 1;
            }

            // try to leave a couple threads available for the GC
            var opts = new ParallelOptions { MaxDegreeOfParallelism = vpus };

            Parallel.ForEach(files, opts, ProcessPhoto);
            
            MagickWandEnvironment.Terminus();
            FinalizeOutputStream();
        }
        
        
        void ProcessPhoto(string file)
        {
            if(!_opts.Quiet)
            {
                Console.WriteLine("Processing: " + Path.GetFileName(file));
            }

            var proc = new PhotoProcessor(SourceResizeTarget, XsResizeTarget, SmResizeTarget, MdResizeTarget, LgResizeTarget);
            var result = proc.ProcessPhotoAsync(file).Result;

            lock(_lockObj)
            {
                AddResultToOutput(result);
            }
        }
        
        
        void PrepareOutputStream()
        {
            _writer = new StreamWriter(new FileStream(_opts.Outfile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8192, FileOptions.None));
        }
        
        
        void FinalizeOutputStream()
        {
            _writer.Flush();
            _writer.Close();
            _writer = null;
        }
        
        
        void AddResultToOutput(ProcessingResult result)
        {
            string priv = _opts.IsPrivate ? "TRUE" : "FALSE";

            if(!_hasSetTeaserPhoto)
            {
                _writer.WriteLine(string.Concat("INSERT INTO photo_category (name, year, is_private, teaser_photo_width, teaser_photo_height, teaser_photo_path) ",
                                                "VALUES (", SqlString(_opts.CategoryName), ", ", _opts.Year, ", ", priv, ", ", result.Xs.Width, ", ", result.Xs.Height, ", ", SqlString(result.Xs.WebPath), ");"));

                _writer.WriteLine();
                _writer.WriteLine(@"SELECT @CATEGORY_ID := LAST_INSERT_ID();");
                _writer.WriteLine();

                _hasSetTeaserPhoto = true;
            }

            _writer.WriteLine("CALL maw_add_photo({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9},"
                                              + " {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19},"
                                              + " {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29},"
                                              + " {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39},"
                                              + " {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49},"
                                              + " {50}, {51}, {52}, {53}, {54}, {55}, {56}, {57}, {58}, {59},"
                                              + " {60}, {61}, {62}, {63}, {64}, {65});",
                              "@CATEGORY_ID",
                              priv,
                              result.Xs.Height,
                              result.Xs.Width,
                              result.Sm.Height,
                              result.Sm.Width,
                              result.Lg.Height,
                              result.Lg.Width,
                              result.Md.Height,
                              result.Md.Width,
                              SqlString(result.Xs.WebPath),
                              SqlString(result.Sm.WebPath),
                              SqlString(result.Lg.WebPath),
                              SqlString(result.Md.WebPath),
                              SqlString(result.Source.WebPath),
                              SqlString(result.ExifData.AutofocusPoint),
                              SqlString(result.ExifData.Aperture),
                              SqlString(result.ExifData.Contrast),
                              SqlString(result.ExifData.DepthOfField),
                              SqlString(result.ExifData.DigitalZoomRatio),
                              SqlString(result.ExifData.ExposureCompensation),
                              SqlString(result.ExifData.ExposureDifference),
                              SqlString(result.ExifData.ExposureMode),
                              SqlString(result.ExifData.ExposureTime),
                              SqlString(result.ExifData.FNumber),
                              SqlString(result.ExifData.Flash),
                              SqlString(result.ExifData.FlashExposureCompensation),
                              SqlString(result.ExifData.FlashMode),
                              SqlString(result.ExifData.FlashSetting),
                              SqlString(result.ExifData.FlashType),
                              SqlString(result.ExifData.FocalLength),
                              SqlString(result.ExifData.FocalLengthIn35mmFormat),
                              SqlString(result.ExifData.FocusDistance),
                              SqlString(result.ExifData.FocusMode),
                              SqlString(result.ExifData.FocusPosition),
                              SqlString(result.ExifData.GainControl),
                              SqlString(result.ExifData.HueAdjustment),
                              SqlString(result.ExifData.HyperfocalDistance),
                              SqlString(result.ExifData.Iso),
                              SqlString(result.ExifData.LensId),
                              SqlString(result.ExifData.LightSource),
                              SqlString(result.ExifData.Make),
                              SqlString(result.ExifData.MeteringMode),
                              SqlString(result.ExifData.Model),
                              SqlString(result.ExifData.NoiseReduction),
                              SqlString(result.ExifData.Orientation),
                              SqlString(result.ExifData.Saturation),
                              SqlString(result.ExifData.ScaleFactor35Efl),
                              SqlString(result.ExifData.SceneCaptureType),
                              SqlString(result.ExifData.SceneType),
                              SqlString(result.ExifData.SensingMethod),
                              SqlString(result.ExifData.Sharpness),
                              SqlString(result.ExifData.ShutterSpeed),
                              SqlString(result.ExifData.WhiteBalance),
                              SqlString(result.ExifData.ShotTakenDate),
                              SqlString(result.ExifData.ExposureProgram),
                              SqlString(result.ExifData.GpsVersionId),
                              SqlString(result.ExifData.GpsLatitude),
                              SqlString(result.ExifData.GpsLatitudeRef),
                              SqlString(result.ExifData.GpsLongitude),
                              SqlString(result.ExifData.GpsLongitudeRef),
                              SqlString(result.ExifData.GpsAltitude),
                              SqlString(result.ExifData.GpsAltitudeRef),
                              SqlString(result.ExifData.GpsDateStamp),
                              SqlString(result.ExifData.GpsTimeStamp),
                              SqlString(result.ExifData.GpsSatellites));
            
            _writer.WriteLine();
        }
        
        
        static string SqlString(string val)
        {
            if(val == null)
            {
                return "NULL";
            }
            else
            {
                return string.Concat("'", val.Replace("'", "''"), "'");
            }
        }
        
        
        static void ShowUsage(SizePhotoOptions options)
        {
            Console.WriteLine(options.CategoryName);
            options.DoHelp();
        }
    }
}
