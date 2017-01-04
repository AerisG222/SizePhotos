using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NExifTool;
using NExifTool.Enums;
using NExifTool.Enums.Gps;


namespace SizePhotos.Exif
{
    public class ExifPhotoProcessor
        : IPhotoProcessor
    {
        const string DATE_FORMAT = "yyyy:MM:dd HH:mm:ss";
        static readonly string[] PREFERRED_SPECIFIC_GROUP_PREFIXES = new string[] { "IFD", "SubIFD1", "SubIFD0", "SubIFD" };
        bool _quiet;
        
        
        public ExifPhotoProcessor(bool quiet)
        {
            _quiet = quiet;
        }
        

        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor) this.MemberwiseClone();
        }


        public async Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext ctx)
        {
            try
            {
                var data = await ReadExifDataAsync(ctx.SourceFile);
                
                return new ExifProcessingResult(true, data);
            }
            catch(Exception ex)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"Error obtaining exif data for file {ctx.SourceFile}.  Error Message: {ex.Message}");
                }

                return new ExifProcessingResult(false, null);
            }
        }


        async Task<ExifData> ReadExifDataAsync(string photoPath)
        {
            var et = new ExifTool(new ExifToolOptions { Quiet = _quiet });
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
        
        
        Tag<T> GetExifData<T>(IEnumerable<Tag> exifData, string datapoint)
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
                if(!_quiet)
                {
                    Console.WriteLine($"error trying to cast tag for {datapoint}.  Was expecting {typeof(T)} but got {t.GetType()} with value {t.Value}");
                }
            }
            
            return null;
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
    }
}
