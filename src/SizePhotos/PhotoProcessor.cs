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
        const int JPG_COMPRESSION_QUALITY = 72;
        const double DARK_THRESHOLD = 250;
        
        
        ResizeTarget RawTarget { get; set; }
        ResizeTarget OriginalTarget { get; set; }
        ResizeTarget ThumbnailTarget { get; set; }
        ResizeTarget FullsizeTarget { get; set; }
        ResizeTarget FullerTarget { get; set; }
        
        
        public PhotoProcessor(ResizeTarget rawTarget, ResizeTarget originalTarget, ResizeTarget thumbnailTarget, 
                              ResizeTarget fullsizeTarget, ResizeTarget fullerTarget)
        {
            RawTarget = rawTarget;
            OriginalTarget = originalTarget;
            ThumbnailTarget = thumbnailTarget;
            FullsizeTarget = fullsizeTarget;
            FullerTarget = fullerTarget;
        }
        
        
        public async Task<PhotoDetail> ProcessPhotoAsync(string photoPath)
        {
            var detail = new PhotoDetail();
            var jpgName = $"{Path.GetFileNameWithoutExtension(photoPath)}.jpg";
            var rawPath = RawTarget.GetLocalPathForPhoto(photoPath);
            string ppmFile = null;
            
            detail.ExifData = await ReadExifData(photoPath);
            
            // always keep the original in the raw dir
            File.Move(photoPath, rawPath);
            detail.RawInfo = new PhotoInfo { WebPath = RawTarget.GetWebPathForPhoto(photoPath) };
            
            using(var wand = new MagickWand())
            {
                if(IsRawFile(photoPath))
                {
                    var dcraw = new DCRaw(GetOptimalOptionsForPhoto(rawPath));
                    ppmFile = (await dcraw.ConvertAsync(rawPath)).OutputFilename;
                    
                    wand.ReadImage(ppmFile);
                    File.Delete(ppmFile);
                } 
                else 
                {
                    wand.ReadImage(rawPath);
                }
                
                wand.AutoOrientImage();
                wand.StripImage();
                wand.ImageCompressionQuality = JPG_COMPRESSION_QUALITY;
                
                using(var tmpWand = wand.Clone())
                {
                    tmpWand.WriteImage(OriginalTarget.GetLocalPathForPhoto(jpgName), true);
                    
                    detail.OriginalInfo = new PhotoInfo {
                        WebPath = OriginalTarget.GetWebPathForPhoto(jpgName),
                        Height = wand.ImageHeight,
                        Width = wand.ImageWidth
                    };
                }
                
                detail.ThumbnailInfo = ScalePhoto(wand, ThumbnailTarget, jpgName);
                detail.FullsizeInfo = ScalePhoto(wand, FullsizeTarget, jpgName);
                detail.FullerInfo = ScalePhoto(wand, FullerTarget, jpgName);
            }
            
            return detail;
        }
        
        
        static PhotoInfo ScalePhoto(MagickWand wand, ResizeTarget target, string jpgName)
        {
            using(var tmpWand = wand.Clone())
            {
                var path = target.GetLocalPathForPhoto(jpgName);
                uint width, height;
                
                wand.GetLargestDimensionsKeepingAspectRatio(target.MaxWidth, target.MaxHeight, out width, out height);
                
                // TODO: might need to adjust compression quality based on scaling amount
                tmpWand.ScaleImage(width, height);
                tmpWand.WriteImage(path, true);
                
                return new PhotoInfo
                {
                    WebPath = target.GetWebPathForPhoto(jpgName),
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
                AutofocusPoint = GetExifData(tags, "AFPoint"),
                Aperture = GetExifData(tags, "Aperture"),
                Contrast = GetExifData(tags, "Contrast"),
                DepthOfField = GetExifData(tags, "DOF"),
                DigitalZoomRatio = GetExifData(tags, "DigitalZoomRatio"),
                ExposureCompensation = GetExifData(tags, "ExposureCompensation"),
                ExposureDifference = GetExifData(tags, "ExposureDifference"),
                ExposureMode = GetExifData(tags, "ExposureMode"),
                ExposureTime = GetExifData(tags, "ExposureTime"),
                FNumber = GetExifData(tags, "FNumber"),
                Flash = GetExifData(tags, "Flash"),
                FlashExposureCompensation = GetExifData(tags, "FlashExposureComp"),
                FlashMode = GetExifData(tags, "FlashMode"),
                FlashSetting = GetExifData(tags, "FlashSetting"),
                FlashType = GetExifData(tags, "FlashType"),
                FocalLength = GetExifData(tags, "FocalLength"),
                FocalLengthIn35mmFormat = GetExifData(tags, "FocalLengthIn35mmFormat"),
                FocusDistance = GetExifData(tags, "FocusDistance"),
                FocusMode = GetExifData(tags, "FocusMode"),
                FocusPosition = GetExifData(tags, "FocusPosition"),
                GainControl = GetExifData(tags, "GainControl"),
                HueAdjustment = GetExifData(tags, "HueAdjustment"),
                HyperfocalDistance = GetExifData(tags, "HyperfocalDistance"),
                Iso = GetExifData(tags, "ISO"),
                LensId = GetExifData(tags, "LensID"),
                LightSource = GetExifData(tags, "LightSource"),
                Make = GetExifData(tags, "Make"),
                MeteringMode = GetExifData(tags, "MeteringMode"),
                Model = GetExifData(tags, "Model"),
                NoiseReduction = GetExifData(tags, "NoiseReduction"),
                Orientation = GetExifData(tags, "Orientation"),
                Saturation = GetExifData(tags, "Saturation"),
                ScaleFactor35Efl = GetExifData(tags, "ScaleFactor35efl"),
                SceneCaptureType = GetExifData(tags, "SceneCaptureType"),
                SceneType = GetExifData(tags, "SceneType"),
                SensingMethod = GetExifData(tags, "SensingMethod"),
                Sharpness = GetExifData(tags, "Sharpness"),
                ShutterSpeed = GetExifData(tags, "ShutterSpeed"),
                WhiteBalance = GetExifData(tags, "WhiteBalance"),
                ShotTakenDate = GetExifData(tags, "DateTimeOriginal"),
                ExposureProgram = GetExifData(tags, "ExposureProgram"),

                // gps datapoints
                GpsVersionId = GetExifData(tags, "GPSVersionID"),
                GpsLatitudeRef = GetExifData(tags, "GPSLatitudeRef"),
                GpsLatitude = GetExifData(tags, "GPSLatitude"),
                GpsLongitudeRef = GetExifData(tags, "GPSLongitudeRef"),
                GpsLongitude = GetExifData(tags, "GPSLongitude"),
                GpsAltitudeRef = GetExifData(tags, "GPSAltitudeRef"),
                GpsAltitude = GetExifData(tags, "GPSAltitude"),
                GpsDateStamp = GetExifData(tags, "GPSDateStamp"),
                GpsTimeStamp = GetExifData(tags, "GPSTimeStamp"),
                GpsSatellites = GetExifData(tags, "GPSSatellites")
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
