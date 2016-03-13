using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDCRaw;
using NExifTool;
using NMagickWand;


namespace SizePhotos
{
    public class PhotoProcessor
    {
        const double DARK_THRESHOLD = 250;
        
        
        ProcessingTarget SourceTarget { get; set; }
        ProcessingTarget XsTarget { get; set; }
        ProcessingTarget SmTarget { get; set; }
        ProcessingTarget MdTarget { get; set; }
        ProcessingTarget LgTarget { get; set; }
        
        
        public PhotoProcessor(ProcessingTarget sourceTarget, ProcessingTarget xsTarget, ProcessingTarget smTarget, 
                              ProcessingTarget mdTarget, ProcessingTarget lgTarget)
        {
            SourceTarget = sourceTarget;
            XsTarget = xsTarget;
            SmTarget = smTarget;
            MdTarget = mdTarget;
            LgTarget = lgTarget;
        }
        
        
        public async Task<ProcessingResult> ProcessPhotoAsync(string photoPath)
        {
            var result = new ProcessingResult();
            var jpgName = $"{Path.GetFileNameWithoutExtension(photoPath)}.jpg";
            var srcPath = SourceTarget.GetLocalPathForPhoto(photoPath);
            string ppmFile = null;
            
            result.ExifData = await ReadExifData(photoPath);
            
            // always keep the original in the source dir
            File.Move(photoPath, srcPath);
            result.Source = new ProcessedPhoto { Target = SourceTarget, Filename = Path.GetFileName(photoPath) };
            
            using(var wand = new MagickWand())
            {
                if(IsRawFile(photoPath))
                {
                    var dcraw = new DCRaw(GetOptimalOptionsForPhoto(srcPath));
                    ppmFile = (await dcraw.ConvertAsync(srcPath)).OutputFilename;
                    
                    wand.ReadImage(ppmFile);
                    File.Delete(ppmFile);
                } 
                else 
                {
                    wand.ReadImage(srcPath);
                }
                
                wand.AutoOrientImage();
                wand.StripImage();
                
                result.Xs = ProcessTarget(wand, XsTarget, jpgName);
                result.Sm = ProcessTarget(wand, SmTarget, jpgName);
                result.Md = ProcessTarget(wand, MdTarget, jpgName);
                result.Lg = ProcessTarget(wand, LgTarget, jpgName);
            }
            
            return result;
        }
        
        
        static ProcessedPhoto ProcessTarget(MagickWand wand, ProcessingTarget target, string jpgName)
        {
            using(var tmpWand = wand.Clone())
            {
                var path = target.GetLocalPathForPhoto(jpgName);
                uint width, height;
                
                wand.GetLargestDimensionsKeepingAspectRatio(target.MaxWidth, target.MaxHeight, out width, out height);
                
                if(target.Optimize)
                {
                    tmpWand.NormalizeImage();
                }
                
                tmpWand.ScaleImage(width, height);
                
                if(target.Quality != null)
                {
                    tmpWand.ImageCompressionQuality = (uint)target.Quality;
                }
                
                tmpWand.WriteImage(path, true);
                
                return new ProcessedPhoto
                {
                    Target = target,
                    Filename = jpgName,
                    Width = width,
                    Height = height
                };
            }
        }
        
        
        static async Task<ExifData> ReadExifData(string photoPath)
        {
            var et = new ExifTool(new ExifToolOptions());
            var tags = await et.GetTagsAsync(photoPath);
            
            return new ExifData {
                // exif
                BitsPerSample = GetExifData("BitsPerSample"),
                Compression = GetExifData("Compression"),
                Contrast = GetExifData(tags, "Contrast"),
                CreateDate = GetExifData("CreateDate"),
                DigitalZoomRatio = GetExifData(tags, "DigitalZoomRatio"),
                ExposureCompensation = GetExifData(tags, "ExposureCompensation"),
                ExposureMode = GetExifData(tags, "ExposureMode"),
                ExposureProgram = GetExifData(tags, "ExposureProgram"),
                ExposureTime = GetExifData(tags, "ExposureTime"),
                FNumber = GetExifData(tags, "FNumber"),
                Flash = GetExifData(tags, "Flash"),
                FocalLength = GetExifData(tags, "FocalLength"),
                FocalLengthIn35mmFormat = GetExifData(tags, "FocalLengthIn35mmFormat"),
                GainControl = GetExifData(tags, "GainControl"),
                GpsAltitude = GetExifData(tags, "GPSAltitude"),
                GpsAltitudeRef = GetExifData(tags, "GPSAltitudeRef"),
                GpsDateStamp = GetExifData(tags, "GPSDateStamp"),
                GPSImgDirection = "GPSImgDirection",
                GPSImgDirectionRef = "GPSImgDirectionRef",
                GpsLatitude = GetExifData(tags, "GPSLatitude"),
                GpsLatitudeRef = GetExifData(tags, "GPSLatitudeRef"),
                GpsLongitude = GetExifData(tags, "GPSLongitude"),
                GpsLongitudeRef = GetExifData(tags, "GPSLongitudeRef"),
                GpsMeasureMode = "GPSMeasureMode",
                GpsSatellites = GetExifData(tags, "GPSSatellites"),
                GpsStatus = "GPSStatus",
                GpsTimeStamp = GetExifData(tags, "GPSTimeStamp"),
                GpsVersionId = GetExifData(tags, "GPSVersionID"),
                Iso = GetExifData(tags, "ISO"),
                LightSource = GetExifData(tags, "LightSource"),
                Make = GetExifData(tags, "Make"),
                MeteringMode = GetExifData(tags, "MeteringMode"),
                Model = GetExifData(tags, "Model"),
                Orientation = GetExifData(tags, "Orientation"),
                SceneCaptureType = GetExifData(tags, "SceneCaptureType"),
                SceneType = GetExifData(tags, "SceneType"),
                SensingMethod = GetExifData(tags, "SensingMethod"),
                Sharpness = GetExifData(tags, "Sharpness"),
                
                // nikon
                AutofocusPoint = GetExifData(tags, "AFPoint"),
                AutoFocusAreaMode = "AFAreaMode",
                ActiveDLighting = "ActiveD-Lighting",
                Brightness = "Brightness",
                Colorspace = "ColorSpace",
                ExposureDifference = GetExifData(tags, "ExposureDifference"),
                FlashColorFilter = "FlashColorFilter",
                FlashCompensation = "FlashCompensation",
                FlashExposureCompensation = GetExifData(tags, "FlashExposureComp"),
                FlashMode = GetExifData(tags, "FlashMode"),
                FlashSetting = GetExifData(tags, "FlashSetting"),
                FlashSource = "FlashSource",
                FlashType = GetExifData(tags, "FlashType"),
                FocusDistance = GetExifData(tags, "FocusDistance"),
                FocusMode = GetExifData(tags, "FocusMode"),
                FocusPosition = GetExifData(tags, "FocusPosition"),
                HighIsoNoiseReduction = "HighIsoNoiseReduction",
                HueAdjustment = GetExifData(tags, "HueAdjustment"),
                NoiseReduction = GetExifData(tags, "NoiseReduction"),
                PictureControlName = "PictureControlName",
                PrimaryAFPoint = "PrimaryAFPoint",
                Saturation = GetExifData(tags, "Saturation"),
                VRMode = "VRMode",
                VibrationReduction = "VibrationReduction",
                VignetteControl = "VignetteControl",
                WhiteBalance = GetExifData(tags, "WhiteBalance"),
                
                // composite
                Aperture = GetExifData(tags, "Aperture"),
                AutoFocus = "AutoFocus",
                DepthOfField = GetExifData(tags, "DOF"),
                FieldOfView = "FOV",
                HyperfocalDistance = GetExifData(tags, "HyperfocalDistance"),
                LensId = GetExifData(tags, "LensID"),
                LightValue = "LightValue",
                ScaleFactor35Efl = GetExifData(tags, "ScaleFactor35efl"),
                ShutterSpeed = GetExifData(tags, "ShutterSpeed"),
            };
        }
        
        
        static string GetExifData(IEnumerable<Tag> exifData, string datapoint)
        {
            var tag = exifData.SingleOrDefault(x => string.Equals(x.TagInfo.Name, datapoint, StringComparison.OrdinalIgnoreCase));
            
            return tag?.Value;
        }
        
                                           
        static bool IsRawFile(string photoPath)
        {
            return !photoPath.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase);
        }
        
        
        static DCRawOptions GetOptimalOptionsForPhoto(string photoPath)
        {
            if(GetDarkThresholdForRawImage(photoPath) < DARK_THRESHOLD)
            {
                Console.WriteLine($"  -using night mode for {photoPath}");
                return GetOptimalNightOptions();
            }
            
            return GetOptimalDayOptions();
        }
        
        
        static double GetDarkThresholdForRawImage(string path)
        {
            var opts = new DCRawOptions {
                HalfSizeColorImage = true,  // try to speed this up, don't need quality here
                UseCameraWhiteBalance = true,
                DontAutomaticallyBrighten = true
            };
            
            var dcraw = new DCRaw(opts);
            var res = dcraw.Convert(path);
            
            using(var wand = new MagickWand(res.OutputFilename))
            {
                double mean, stddev;
                
                // read the image, posterize, then figure out how 'dark' it is
                wand.PosterizeImage(3, false);
                wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
                
                File.Delete(res.OutputFilename);

                return mean;
            }
        }
        
        
        static DCRawOptions GetOptimalDayOptions()
        {
            return new DCRawOptions {
                UseCameraWhiteBalance = true,
                Quality = InterpolationQuality.Quality3,
                HighlightMode = HighlightMode.Blend,
                Colorspace = Colorspace.sRGB
            };
        }
        
        
        static DCRawOptions GetOptimalNightOptions()
        {
            var opts = GetOptimalDayOptions();
            
            opts.DontAutomaticallyBrighten = true;
            
            return opts;
        }
    }
}
