using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDCRaw;
using NExifTool;
using NExifTool.Enums;
using NExifTool.Enums.Gps;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos
{
    public class PhotoProcessor
    {
        const double DARK_THRESHOLD = 250;
        const string DATE_FORMAT = "yyyy:MM:dd HH:mm:ss";
        static readonly object _lockobj = new object();
        static readonly string[] PREFERRED_SPECIFIC_GROUP_PREFIXES = new string[] { "IFD", "SubIFD1", "SubIFD0", "SubIFD" };
        
        
        PhotoPathHelper PathHelper { get; set; }
        ProcessingTarget SourceTarget { get; set; }
        ProcessingTarget XsTarget { get; set; }
        ProcessingTarget SmTarget { get; set; }
        ProcessingTarget MdTarget { get; set; }
        ProcessingTarget LgTarget { get; set; }
        bool Quiet { get; set; }
        
        
        public PhotoProcessor(PhotoPathHelper pathHelper, ProcessingTarget sourceTarget, ProcessingTarget xsTarget,  
                              ProcessingTarget smTarget, ProcessingTarget mdTarget, ProcessingTarget lgTarget, bool quiet)
        {
            PathHelper = pathHelper;
            SourceTarget = sourceTarget;
            XsTarget = xsTarget;
            SmTarget = smTarget;
            MdTarget = mdTarget;
            LgTarget = lgTarget;
            Quiet = quiet;
        }
        
        
        public async Task<ProcessingResult> ProcessPhotoAsync(string filename)
        {
            var result = new ProcessingResult();
            var jpgName = Path.ChangeExtension(filename, ".jpg");
            var origPath = PathHelper.GetSourceFilePath(filename);
            var srcPath = PathHelper.GetScaledLocalPath(SourceTarget.ScaledPathSegment, filename);
            string ppmFile = null;
            
            result.ExifData = await ReadExifData(origPath);
            
            // always keep the original in the source dir
            File.Move(origPath, srcPath);
            result.Source = new ProcessedPhoto { 
                Target = SourceTarget, 
                LocalFilePath = srcPath, 
                WebFilePath = PathHelper.GetScaledWebFilePath(SourceTarget.ScaledPathSegment, filename)
            };
            
            using(var wand = new MagickWand())
            {
                if(IsRawFile(filename))
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
        
        
        ProcessedPhoto ProcessTarget(MagickWand wand, ProcessingTarget target, string jpgName)
        {
            var contrastRatioThreshold = 0.5;
            var tooContrastyThreshold = 1.9;
            
            using(var tmpWand = wand.Clone())
            {
                var path = PathHelper.GetScaledLocalPath(target.ScaledPathSegment, jpgName);
                uint width, height;
                
                wand.GetLargestDimensionsKeepingAspectRatio(target.MaxWidth, target.MaxHeight, out width, out height);
                
                if(target.Optimize)
                {
                    double mean, stddev;
                    
                    wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
                                        
                    wand.AutoLevelImage();
                    
                    var contrastRatio = stddev / mean;
                    var contrastyRatio = stddev / 10000;
                    
                    if(contrastRatio < contrastRatioThreshold)
                    {
                        var saturationAmount = Convert.ToInt32((contrastRatioThreshold - contrastRatio) * 100) * 4;
                        
                        // limit the saturation adjustment to 20%
                        if(saturationAmount > 20)
                        {
                            saturationAmount = 20;
                        }
                        
                        saturationAmount += 100;
                        
                        // 100 = don't adjust brightness
                        // 300 = don't rotate hue
                        wand.ModulateImage(100, saturationAmount, 300);
                    }
                    else if(contrastyRatio > tooContrastyThreshold)
                    {
                        wand.SigmoidalContrastImage(true, 2, 0);  // smooth brightness/contrast
                    }
                }
                
                tmpWand.ScaleImage(width, height);
                tmpWand.UnsharpMaskImage(0, 0.7, 0.7, 0.008);

                if(target.Quality != null)
                {
                    tmpWand.ImageCompressionQuality = (uint)target.Quality;
                }
                
                tmpWand.WriteImage(path, true);
                
                return new ProcessedPhoto
                {
                    Target = target,
                    LocalFilePath = jpgName, 
                    WebFilePath = PathHelper.GetScaledWebFilePath(target.ScaledPathSegment, jpgName),
                    Width = width,
                    Height = height
                };
            }
        }
        
        
        async Task<ExifData> ReadExifData(string photoPath)
        {
            var et = new ExifTool(new ExifToolOptions { Quiet = Quiet });
            var tags = await et.GetTagsAsync(photoPath);
            
            // NOTE: if we ever add a new explicit type below, please be sure to add a fallback in GetExifData<T>
            return new ExifData {
                // exif
                BitsPerSample = GetExifData<ushort>(tags, "BitsPerSample")?.TypedValue,
                Compression = GetExifData<Compression>(tags, "Compression")?.TypedValue.Key,
                Contrast = GetExifData<Contrast>(tags, "Contrast")?.TypedValue.Key,
                CreateDate = GetExifDate(tags, "CreateDate"),
                DigitalZoomRatio = GetExifData<double>(tags, "DigitalZoomRatio")?.TypedValue,
                ExposureCompensation = GetExifData(tags, "ExposureCompensation")?.Value,
                ExposureMode = GetExifData<ExposureMode>(tags, "ExposureMode")?.TypedValue.Key,
                ExposureProgram = GetExifData<ExposureProgram>(tags, "ExposureProgram")?.TypedValue.Key,
                ExposureTime = GetExifData(tags, "ExposureTime")?.Value,
                FNumber = GetExifData<double>(tags, "FNumber")?.TypedValue,
                Flash = GetExifData<int>(tags, "Flash")?.TypedValue,
                FocalLength = GetExifData<double>(tags, "FocalLength")?.TypedValue,
                FocalLengthIn35mmFormat = GetExifData<double>(tags, "FocalLengthIn35mmFormat")?.TypedValue,
                GainControl = GetExifData<GainControl>(tags, "GainControl")?.TypedValue.Key,
                GpsAltitude = GetExifData<double>(tags, "GPSAltitude")?.TypedValue,
                GpsAltitudeRef = GetExifData<GpsAltitudeRef>(tags, "GPSAltitudeRef")?.TypedValue.Key,
                GpsDateStamp = GetExifDate(tags, "GPSDateStamp"),
                GpsDirection = GetExifData<double>(tags, "GPSImgDirection")?.TypedValue,
                GpsDirectionRef = GetExifData<GpsImgDirectionRef>(tags, "GPSImgDirectionRef")?.TypedValue.Key,
                GpsLatitude = GetExifData<double>(tags, "GPSLatitude")?.TypedValue,
                GpsLatitudeRef = GetExifData<GpsLatitudeRef>(tags, "GPSLatitudeRef")?.TypedValue.Key,
                GpsLongitude = GetExifData<double>(tags, "GPSLongitude")?.TypedValue,
                GpsLongitudeRef = GetExifData<GpsLongitudeRef>(tags, "GPSLongitudeRef")?.TypedValue.Key,
                GpsMeasureMode = GetExifData<GpsMeasureMode>(tags, "GPSMeasureMode")?.TypedValue.Key,
                GpsSatellites = GetExifData(tags, "GPSSatellites")?.Value,
                GpsStatus = GetExifData<GpsStatus>(tags, "GPSStatus")?.TypedValue.Key,
                GpsVersionId = GetExifData(tags, "GPSVersionID")?.Value,
                Iso = GetExifData<int>(tags, "ISO")?.TypedValue,
                LightSource = GetExifData<LightSource>(tags, "LightSource")?.TypedValue.Key,
                Make = GetExifData(tags, "Make")?.Value,
                MeteringMode = GetExifData<MeteringMode>(tags, "MeteringMode")?.TypedValue.Key,
                Model = GetExifData(tags, "Model")?.Value,
                Orientation = GetExifData<Orientation>(tags, "Orientation")?.TypedValue.Key,
                Saturation = GetExifData(tags, "Saturation")?.Value,
                SceneCaptureType = GetExifData<SceneCaptureType>(tags, "SceneCaptureType")?.TypedValue.Key,
                SceneType = GetExifData<SceneType>(tags, "SceneType")?.TypedValue.Key,
                SensingMethod = GetExifData<SensingMethod>(tags, "SensingMethod")?.TypedValue.Key,
                Sharpness = GetExifData<Sharpness>(tags, "Sharpness")?.TypedValue.Key,
                
                // nikon
                AutoFocusAreaMode = GetExifData(tags, "AFAreaMode")?.Value,
                AutoFocusPoint = GetExifData(tags, "AFPoint")?.Value,
                ActiveDLighting = GetExifData(tags, "ActiveD-Lighting")?.Value,
                Colorspace = GetExifData(tags, "ColorSpace")?.Value,
                ExposureDifference = GetExifData<double>(tags, "ExposureDifference")?.TypedValue,
                FlashColorFilter = GetExifData(tags, "FlashColorFilter")?.Value,
                FlashCompensation = GetExifData(tags, "FlashCompensation")?.Value,
                FlashControlMode = GetExifData<short>(tags, "FlashControlMode")?.TypedValue,
                FlashExposureCompensation = GetExifData(tags, "FlashExposureComp")?.Value,
                FlashFocalLength = GetExifData<double>(tags, "FlashFocalLength")?.TypedValue,
                FlashMode = GetExifData(tags, "FlashMode")?.Value,
                FlashSetting = GetExifData(tags, "FlashSetting")?.Value,
                FlashType = GetExifData(tags, "FlashType")?.Value,
                FocusDistance = GetExifData<double>(tags, "FocusDistance")?.TypedValue,
                FocusMode = GetExifData(tags, "FocusMode")?.Value,
                FocusPosition = GetExifData<int>(tags, "FocusPosition")?.TypedValue,
                HighIsoNoiseReduction = GetExifData(tags, "HighIsoNoiseReduction")?.Value,
                HueAdjustment = GetExifData(tags, "HueAdjustment")?.Value,
                NoiseReduction = GetExifData(tags, "NoiseReduction")?.Value,
                PictureControlName = GetExifData(tags, "PictureControlName")?.Value,
                PrimaryAFPoint = GetExifData(tags, "PrimaryAFPoint")?.Value,
                VRMode = GetExifData(tags, "VRMode")?.Value,
                VibrationReduction = GetExifData(tags, "VibrationReduction")?.Value,
                VignetteControl = GetExifData(tags, "VignetteControl")?.Value,
                WhiteBalance = GetExifData(tags, "WhiteBalance")?.Value,
                
                // composite
                Aperture = GetExifData<double>(tags, "Aperture")?.TypedValue,
                AutoFocus = GetExifData<short>(tags, "AutoFocus")?.TypedValue,
                DepthOfField = GetExifData(tags, "DOF")?.Value,
                FieldOfView = GetExifData(tags, "FOV")?.Value,
                HyperfocalDistance = GetExifData<double>(tags, "HyperfocalDistance")?.TypedValue,
                LensId = GetExifData(tags, "LensID")?.Value,
                LightValue = GetExifData<double>(tags, "LightValue")?.TypedValue,
                ScaleFactor35Efl = GetExifData<double>(tags, "ScaleFactor35efl")?.TypedValue,
                ShutterSpeed = GetExifData(tags, "ShutterSpeed")?.Value,
            };
        }
        
        
        static Tag<T> GetExifData<T>(IEnumerable<Tag> exifData, string datapoint)
        {
            var t = GetExifData(exifData, datapoint);
            
            if(t == null)
            {
                return null;
            }
            
            var tt = t as Tag<T>;
            
            if(tt != null)
            {
                return tt;
            }
            
            // sometimes we have cases where similar tags are in different exif 'tables'.  this results in different
            // types for the same tag.  a good example of this is BitsPerSample.  For nikon, this is an int8u but an int16u for JPG
            // this tries to jam our number into the requested type (right now, only supports numerics)
            //
            // NOTE: if we ever add a new explicit type above, please be sure to add a fallback here for added safety
            try 
            {
                var val = (T)Convert.ChangeType(t.NumberValue, typeof(T));
                
                return new Tag<T> 
                {
                    TagInfo = t.TagInfo,
                    Value = t.Value,
                    NumberValue = t.NumberValue,
                    TypedValue = val
                };
            }
            catch
            {
                //return null;
                throw new InvalidDataException($"error trying to cast tag for {datapoint}.  Was expecting {typeof(T)} but got {t.GetType()} with value {t.Value}");    
            }
        }
        
        
        static DateTime? GetExifDate(IEnumerable<Tag> exifData, string datapoint)
        {
            DateTime dt;
            var t = GetExifData(exifData, datapoint);
            
            if(t != null && DateTime.TryParseExact(t.Value, DATE_FORMAT, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AllowWhiteSpaces, out dt))
            {
                return dt;
            }

            return null;
        }
        
        
        static Tag GetExifData(IEnumerable<Tag> exifData, string datapoint)
        {
            var tags = exifData.Where(x => string.Equals(x.TagInfo.Name, datapoint, StringComparison.OrdinalIgnoreCase));
            
            if(tags.Count() > 1)
            {
                foreach(var pref in PREFERRED_SPECIFIC_GROUP_PREFIXES)
                {
                    var tag = tags.FirstOrDefault(x => x.TagInfo.SpecificGroup.StartsWith(pref, StringComparison.OrdinalIgnoreCase));
                    
                    if(tag != null) 
                    {
                        return tag;
                    }
                }
            }
            
            return tags.FirstOrDefault();
        }
        
                                           
        static bool IsRawFile(string photoPath)
        {
            return photoPath.EndsWith("nef", StringComparison.InvariantCultureIgnoreCase);
        }
        
        
        DCRawOptions GetOptimalOptionsForPhoto(string photoPath)
        {
            if(GetDarkThresholdForRawImage(photoPath) < DARK_THRESHOLD)
            {
                if(!Quiet)
                {
                    Console.WriteLine($"  -using night mode for {photoPath}");    
                }
                
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
