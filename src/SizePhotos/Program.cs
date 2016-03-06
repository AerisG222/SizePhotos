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
        readonly object _lockObj = new object();
        StreamWriter _writer;
        bool _hasSetTeaserPhoto;
        SizePhotoOptions _opts;
        ResizeTarget RawResizeTarget { get; set; }
        ResizeTarget OriginalResizeTarget { get; set; }
        ResizeTarget ThumbnailResizeTarget { get; set; }
        ResizeTarget FullsizeResizeTarget { get; set; }
        ResizeTarget FullerResizeTarget { get; set; }
        
        
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
            RawResizeTarget = GetResizeTarget("raw", 0, 0);
            OriginalResizeTarget = GetResizeTarget("orig", 0, 0);
            ThumbnailResizeTarget = GetResizeTarget("thumbnails", 120, 160);
            FullsizeResizeTarget = GetResizeTarget("fullsize", 480, 640);
            FullerResizeTarget = GetResizeTarget("fuller", 768, 1024);
        }
        
        
        ResizeTarget GetResizeTarget(string pathSegment, uint maxHeight, uint maxWidth)
        {
            return new ResizeTarget {
                LocalPath = _opts.GetLocalScaledPath(pathSegment),
                WebPath = _opts.GetWebScaledPath(pathSegment),
                MaxHeight = maxHeight,
                MaxWidth = maxWidth
            };
        }
        
        
        void PrepareDirectories()
        {
            var targets = new List<ResizeTarget> { 
                RawResizeTarget,
                OriginalResizeTarget,
                ThumbnailResizeTarget,
                FullsizeResizeTarget,
                FullerResizeTarget
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

            var proc = new PhotoProcessor(RawResizeTarget, OriginalResizeTarget, ThumbnailResizeTarget,
                                          FullsizeResizeTarget, FullerResizeTarget);
            var detail = proc.ProcessPhotoAsync(file).Result;

            lock(_lockObj)
            {
                AddResultToOutput(detail);
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
        
        
        void AddResultToOutput(PhotoDetail detail)
        {
            string priv = _opts.IsPrivate ? "TRUE" : "FALSE";

            if(!_hasSetTeaserPhoto)
            {
                _writer.WriteLine(string.Concat("INSERT INTO photo_category (name, year, is_private, teaser_photo_width, teaser_photo_height, teaser_photo_path) ",
                                                "VALUES (", SqlString(_opts.CategoryName), ", ", _opts.Year, ", ", priv, ", ", detail.ThumbnailInfo.Width, ", ", detail.ThumbnailInfo.Height, ", ", SqlString(detail.ThumbnailInfo.WebPath), ");"));

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
                              detail.ThumbnailInfo.Height,
                              detail.ThumbnailInfo.Width,
                              detail.FullsizeInfo.Height,
                              detail.FullsizeInfo.Width,
                              detail.OriginalInfo.Height,
                              detail.OriginalInfo.Width,
                              detail.FullerInfo.Height,
                              detail.FullerInfo.Width,
                              SqlString(detail.ThumbnailInfo.WebPath),
                              SqlString(detail.FullsizeInfo.WebPath),
                              SqlString(detail.OriginalInfo.WebPath),
                              SqlString(detail.FullerInfo.WebPath),
                              SqlString(detail.RawInfo.WebPath),
                              SqlString(detail.ExifData.AutofocusPoint),
                              SqlString(detail.ExifData.Aperture),
                              SqlString(detail.ExifData.Contrast),
                              SqlString(detail.ExifData.DepthOfField),
                              SqlString(detail.ExifData.DigitalZoomRatio),
                              SqlString(detail.ExifData.ExposureCompensation),
                              SqlString(detail.ExifData.ExposureDifference),
                              SqlString(detail.ExifData.ExposureMode),
                              SqlString(detail.ExifData.ExposureTime),
                              SqlString(detail.ExifData.FNumber),
                              SqlString(detail.ExifData.Flash),
                              SqlString(detail.ExifData.FlashExposureCompensation),
                              SqlString(detail.ExifData.FlashMode),
                              SqlString(detail.ExifData.FlashSetting),
                              SqlString(detail.ExifData.FlashType),
                              SqlString(detail.ExifData.FocalLength),
                              SqlString(detail.ExifData.FocalLengthIn35mmFormat),
                              SqlString(detail.ExifData.FocusDistance),
                              SqlString(detail.ExifData.FocusMode),
                              SqlString(detail.ExifData.FocusPosition),
                              SqlString(detail.ExifData.GainControl),
                              SqlString(detail.ExifData.HueAdjustment),
                              SqlString(detail.ExifData.HyperfocalDistance),
                              SqlString(detail.ExifData.Iso),
                              SqlString(detail.ExifData.LensId),
                              SqlString(detail.ExifData.LightSource),
                              SqlString(detail.ExifData.Make),
                              SqlString(detail.ExifData.MeteringMode),
                              SqlString(detail.ExifData.Model),
                              SqlString(detail.ExifData.NoiseReduction),
                              SqlString(detail.ExifData.Orientation),
                              SqlString(detail.ExifData.Saturation),
                              SqlString(detail.ExifData.ScaleFactor35Efl),
                              SqlString(detail.ExifData.SceneCaptureType),
                              SqlString(detail.ExifData.SceneType),
                              SqlString(detail.ExifData.SensingMethod),
                              SqlString(detail.ExifData.Sharpness),
                              SqlString(detail.ExifData.ShutterSpeed),
                              SqlString(detail.ExifData.WhiteBalance),
                              SqlString(detail.ExifData.ShotTakenDate),
                              SqlString(detail.ExifData.ExposureProgram),
                              SqlString(detail.ExifData.GpsVersionId),
                              SqlString(detail.ExifData.GpsLatitude),
                              SqlString(detail.ExifData.GpsLatitudeRef),
                              SqlString(detail.ExifData.GpsLongitude),
                              SqlString(detail.ExifData.GpsLongitudeRef),
                              SqlString(detail.ExifData.GpsAltitude),
                              SqlString(detail.ExifData.GpsAltitudeRef),
                              SqlString(detail.ExifData.GpsDateStamp),
                              SqlString(detail.ExifData.GpsTimeStamp),
                              SqlString(detail.ExifData.GpsSatellites));
            
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
