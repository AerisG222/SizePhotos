using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NExifTool;


namespace SizePhotos.Exif
{
    public class ExifPhotoProcessor
        : IPhotoProcessor
    {
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
                return new ExifProcessingResult($"Error obtaining exif data: {ex.Message}");
            }
        }


        async Task<ExifData> ReadExifDataAsync(string photoPath)
        {
            var et = new ExifTool(new ExifToolOptions());
            var tags = await et.GetTagsAsync(photoPath);

            // NOTE: if we ever add a new explicit type below, please be sure to add a fallback in GetExifData<T>
            return new ExifData {
                // exif
                BitsPerSample = tags.SingleOrDefaultPrimaryTag("BitsPerSample")?.TryGetUInt16(),
                Compression = tags.SingleOrDefaultPrimaryTag("Compression")?.TryGetInt32(),
                Contrast = tags.SingleOrDefaultPrimaryTag("Contrast")?.TryGetInt32(),
                CreateDate = tags.SingleOrDefaultPrimaryTag("CreateDate")?.TryGetDateTime(),
                DigitalZoomRatio = tags.SingleOrDefaultPrimaryTag("DigitalZoomRatio")?.TryGetDouble(),
                ExposureCompensation = tags.SingleOrDefaultPrimaryTag("ExposureCompensation")?.Value,
                ExposureMode = tags.SingleOrDefaultPrimaryTag("ExposureMode")?.TryGetInt32(),
                ExposureProgram = tags.SingleOrDefaultPrimaryTag("ExposureProgram")?.TryGetInt32(),
                ExposureTime = tags.SingleOrDefaultPrimaryTag("ExposureTime")?.Value,
                FNumber = tags.SingleOrDefaultPrimaryTag("FNumber")?.TryGetDouble(),
                Flash = tags.SingleOrDefaultPrimaryTag("Flash")?.TryGetInt32(),
                FocalLength = tags.SingleOrDefaultPrimaryTag("FocalLength")?.TryGetDouble(),
                FocalLengthIn35mmFormat = tags.SingleOrDefaultPrimaryTag("FocalLengthIn35mmFormat")?.TryGetDouble(),
                GainControl = tags.SingleOrDefaultPrimaryTag("GainControl")?.TryGetInt32(),
                GpsAltitude = tags.SingleOrDefaultPrimaryTag("GPSAltitude")?.TryGetDouble(),
                GpsAltitudeRef = tags.SingleOrDefaultPrimaryTag("GPSAltitudeRef")?.Value,
                GpsDateStamp = tags.SingleOrDefaultPrimaryTag("GPSDateStamp")?.TryGetDateTime(),
                GpsDirection = tags.SingleOrDefaultPrimaryTag("GPSImgDirection")?.TryGetDouble(),
                GpsDirectionRef = tags.SingleOrDefaultPrimaryTag("GPSImgDirectionRef")?.NumberValue,
                GpsLatitude = tags.SingleOrDefaultPrimaryTag("GPSLatitude")?.TryGetDouble(),
                GpsLatitudeRef = tags.SingleOrDefaultPrimaryTag("GPSLatitudeRef")?.Value?.Substring(0, 1),
                GpsLongitude = tags.SingleOrDefaultPrimaryTag("GPSLongitude")?.TryGetDouble(),
                GpsLongitudeRef = tags.SingleOrDefaultPrimaryTag("GPSLongitudeRef")?.Value?.Substring(0, 1),
                GpsMeasureMode = tags.SingleOrDefaultPrimaryTag("GPSMeasureMode")?.Value,
                GpsSatellites = tags.SingleOrDefaultPrimaryTag("GPSSatellites")?.Value,
                GpsStatus = tags.SingleOrDefaultPrimaryTag("GPSStatus")?.Value,
                GpsVersionId = tags.SingleOrDefaultPrimaryTag("GPSVersionID")?.Value,
                Iso = tags.SingleOrDefaultPrimaryTag("ISO")?.TryGetInt32(),
                LightSource = tags.SingleOrDefaultPrimaryTag("LightSource")?.TryGetInt32(),
                Make = tags.SingleOrDefaultPrimaryTag("Make")?.Value,
                MeteringMode = tags.SingleOrDefaultPrimaryTag("MeteringMode")?.TryGetInt32(),
                Model = tags.SingleOrDefaultPrimaryTag("Model")?.Value,
                Orientation = tags.SingleOrDefaultPrimaryTag("Orientation")?.TryGetInt32(),
                Saturation = tags.SingleOrDefaultPrimaryTag("Saturation")?.Value,
                SceneCaptureType = tags.SingleOrDefaultPrimaryTag("SceneCaptureType")?.TryGetInt32(),
                SceneType = tags.SingleOrDefaultPrimaryTag("SceneType")?.TryGetInt32(),
                SensingMethod = tags.SingleOrDefaultPrimaryTag("SensingMethod")?.TryGetInt32(),
                Sharpness = tags.SingleOrDefaultPrimaryTag("Sharpness")?.TryGetInt32(),

                // nikon
                AutoFocusAreaMode = tags.SingleOrDefaultPrimaryTag("AFAreaMode")?.Value,
                AutoFocusPoint = tags.SingleOrDefaultPrimaryTag("AFPoint")?.Value,
                ActiveDLighting = tags.SingleOrDefaultPrimaryTag("ActiveD-Lighting")?.Value,
                Colorspace = tags.SingleOrDefaultPrimaryTag("ColorSpace")?.Value,
                ExposureDifference = tags.SingleOrDefaultPrimaryTag("ExposureDifference")?.TryGetDouble(),
                FlashColorFilter = tags.SingleOrDefaultPrimaryTag("FlashColorFilter")?.Value,
                FlashCompensation = tags.SingleOrDefaultPrimaryTag("FlashCompensation")?.Value,
                FlashControlMode = tags.SingleOrDefaultPrimaryTag("FlashControlMode")?.TryGetInt16(),
                FlashExposureCompensation =tags.SingleOrDefaultPrimaryTag("FlashExposureComp")?.Value,
                FlashFocalLength = tags.SingleOrDefaultPrimaryTag("FlashFocalLength")?.TryGetDouble(),
                FlashMode = tags.SingleOrDefaultPrimaryTag("FlashMode")?.Value,
                FlashSetting = tags.SingleOrDefaultPrimaryTag("FlashSetting")?.Value,
                FlashType = tags.SingleOrDefaultPrimaryTag("FlashType")?.Value,
                FocusDistance = tags.SingleOrDefaultPrimaryTag("FocusDistance")?.TryGetDouble(),
                FocusMode = tags.SingleOrDefaultPrimaryTag("FocusMode")?.Value,
                FocusPosition = tags.SingleOrDefaultPrimaryTag("FocusPosition")?.TryGetInt32(),
                HighIsoNoiseReduction = tags.SingleOrDefaultPrimaryTag("HighIsoNoiseReduction")?.Value,
                HueAdjustment = tags.SingleOrDefaultPrimaryTag("HueAdjustment")?.Value,
                NoiseReduction = tags.SingleOrDefaultPrimaryTag("NoiseReduction")?.Value,
                PictureControlName = tags.SingleOrDefaultPrimaryTag("PictureControlName")?.Value,
                PrimaryAFPoint = tags.SingleOrDefaultPrimaryTag("PrimaryAFPoint")?.Value,
                VRMode = tags.SingleOrDefaultPrimaryTag("VRMode")?.Value,
                VibrationReduction = tags.SingleOrDefaultPrimaryTag("VibrationReduction")?.Value,
                VignetteControl = tags.SingleOrDefaultPrimaryTag("VignetteControl")?.Value,
                WhiteBalance = tags.SingleOrDefaultPrimaryTag("WhiteBalance")?.Value,

                // composite
                Aperture = tags.SingleOrDefaultPrimaryTag("Aperture")?.TryGetDouble(),
                AutoFocus = tags.SingleOrDefaultPrimaryTag("AutoFocus")?.TryGetInt16(),
                DepthOfField = tags.SingleOrDefaultPrimaryTag("DOF")?.Value,
                FieldOfView = tags.SingleOrDefaultPrimaryTag("FOV")?.Value,
                HyperfocalDistance = tags.SingleOrDefaultPrimaryTag("HyperfocalDistance")?.TryGetDouble(),
                LensId = tags.SingleOrDefaultPrimaryTag("LensID")?.Value,
                LightValue = tags.SingleOrDefaultPrimaryTag("LightValue")?.TryGetDouble(),
                ScaleFactor35Efl = tags.SingleOrDefaultPrimaryTag("ScaleFactor35efl")?.TryGetDouble(),
                ShutterSpeed = tags.SingleOrDefaultPrimaryTag("ShutterSpeed")?.Value,
            };
        }
    }
}
