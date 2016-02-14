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
        
        
        public async Task<PhotoDetail> ProcessPhoto(string photoPath)
        {
            var detail = new PhotoDetail();
            var jpgName = $"{Path.GetFileNameWithoutExtension(photoPath)}.jpg";
            var rawPath = RawTarget.GetLocalPathForPhoto(photoPath);
            var wand = MagickWandApi.NewMagickWand();
            string ppmFile = null;
            
            detail.ExifData = await ReadExifData(photoPath);
            
            // always keep the original in the raw dir
            File.Move(photoPath, rawPath);
            detail.RawInfo = new PhotoInfo { WebPath = RawTarget.GetWebPathForPhoto(photoPath) };
            
            if(IsRawFile(photoPath))
            {
                // nef -> ppm
                var dcraw = new DCRaw(GetOptimalOptionsForPhoto(rawPath));
                ppmFile = (await dcraw.ConvertAsync(rawPath)).OutputFilename;
                
                // read the result
                MagickWandApi.MagickReadImage(wand, ppmFile);

                // kill the ppm file as we've already read it into mem
                File.Delete(ppmFile);
            } 
            else 
            {
                // read the source file into our wand
                MagickWandApi.MagickReadImage(wand, rawPath);
            }
            
            MagickWandApi.MagickStripImage(wand);
            MagickWandApi.MagickSetImageCompressionQuality(wand, (UIntPtr)JPG_COMPRESSION_QUALITY);
            MagickWandApi.MagickWriteImage(wand, OriginalTarget.GetLocalPathForPhoto(jpgName));
                
            detail.OriginalInfo = new PhotoInfo {
                WebPath = OriginalTarget.GetWebPathForPhoto(jpgName),
                Height = (uint)MagickWandApi.MagickGetImageHeight(wand),
                Width = (uint)MagickWandApi.MagickGetImageWidth(wand)
            };
            
            uint height;
            uint width;
            
            ScalePhoto(wand, detail.OriginalInfo.Height, detail.OriginalInfo.Width, ThumbnailTarget.MaxHeight, ThumbnailTarget.MaxWidth, ThumbnailTarget.GetLocalPathForPhoto(jpgName), out height, out width);
            detail.ThumbnailInfo = new PhotoInfo {
                WebPath = ThumbnailTarget.GetWebPathForPhoto(jpgName),
                Height = height,
                Width = width
            };
            
            ScalePhoto(wand, detail.OriginalInfo.Height, detail.OriginalInfo.Width, FullsizeTarget.MaxHeight, FullsizeTarget.MaxWidth, FullsizeTarget.GetLocalPathForPhoto(jpgName), out height, out width);
            detail.FullsizeInfo = new PhotoInfo {
                WebPath = FullsizeTarget.GetWebPathForPhoto(jpgName),
                Height = height,
                Width = width
            };
            
            ScalePhoto(wand, detail.OriginalInfo.Height, detail.OriginalInfo.Width, FullerTarget.MaxHeight, FullerTarget.MaxWidth, FullerTarget.GetLocalPathForPhoto(jpgName), out height, out width);
            detail.FullerInfo = new PhotoInfo {
                WebPath = FullerTarget.GetWebPathForPhoto(jpgName),
                Height = height,
                Width = width
            };
            
            MagickWandApi.DestroyMagickWand(wand);

            return detail;
        }
        
        
        static void ScalePhoto(IntPtr wand, uint origHeight, uint origWidth, uint maxHeight, uint maxWidth, string path, out uint height, out uint width)
        {
            var tmpWand = MagickWandApi.CloneMagickWand(wand);
            
            // TODO: might need to adjust compression quality based on scaling amount
            GetScaledDimensions(origHeight, origWidth, maxHeight, maxWidth, out height, out width);
            MagickWandApi.MagickScaleImage(tmpWand, (UIntPtr)width, (UIntPtr)height);
            MagickWandApi.MagickWriteImage(tmpWand, path);
            
            MagickWandApi.DestroyMagickWand(tmpWand);
        }
        
        
        static void GetScaledDimensions(uint height, uint width, uint maxScaledHeight, uint maxScaledWidth, out uint scaledHeight, out uint scaledWidth)
        {
            double origRatio = (double) height / (double) width;
            double requestedRatio = (double) maxScaledHeight / (double) maxScaledWidth;
            
            if(origRatio >= requestedRatio)
            {
                // height will be the max value, reset the width
                scaledHeight = maxScaledHeight;
                scaledWidth = Convert.ToUInt32(((double) 1 / origRatio) * maxScaledHeight);
            }
            else
            {
                // width will be the max value, reset the height
                scaledWidth = maxScaledWidth;
                scaledHeight = Convert.ToUInt32(origRatio * (double) maxScaledWidth);
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
            
            var wand = MagickWandApi.NewMagickWand();
            double mean = 0;
            double stddev = 0;
            
            // read the image, posterize, then figure out how 'dark' it is
            MagickWandApi.MagickReadImage(wand, res.OutputFilename);
            MagickWandApi.MagickPosterizeImage(wand, (UIntPtr)3, MagickBooleanType.False);
            MagickWandApi.MagickGetImageChannelMean(wand, ChannelType.AllChannels, ref mean, ref stddev);
            
            MagickWandApi.DestroyMagickWand(wand);
            
            return mean;
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
            return new DCRawOptions {
                UseCameraWhiteBalance = true,
                Quality = InterpolationQuality.Quality3,
                HighlightMode = HighlightMode.Blend,
                Colorspace = Colorspace.sRGB,
                DontAutomaticallyBrighten = true
            };
        }
    }
}
