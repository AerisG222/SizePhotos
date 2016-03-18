using System;


namespace SizePhotos
{
    public class ExifData
    {
        // exif
        public ushort? BitsPerSample { get; set; }
        public int? Compression { get; set; }
        public int? Contrast { get; set; }
        public DateTime? CreateDate { get; set; }
        public double? DigitalZoomRatio { get; set; }
        public string ExposureCompensation { get; set; }
        public int? ExposureMode { get; set; }
        public int? ExposureProgram { get; set; }
        public string ExposureTime { get; set; }
        public double? FNumber { get; set; }
        public int? Flash { get; set; }
        public double? FocalLength { get; set; }
        public double? FocalLengthIn35mmFormat { get; set; }
        public int? GainControl { get; set; }
        public double? GpsAltitude { get; set; }
        public short? GpsAltitudeRef { get; set; }
        public DateTime? GpsDateStamp { get; set; }
        public double? GpsDirection { get; set; }
        public string GpsDirectionRef { get; set; }
        public double? GpsLatitude { get; set; }
        public string GpsLatitudeRef { get; set; }
        public double? GpsLongitude { get; set; }
        public string GpsLongitudeRef { get; set; }
        public string GpsMeasureMode { get; set; }
        public string GpsSatellites { get; set; }
        public string GpsStatus { get; set; }
        public string GpsVersionId { get; set; }
        public int? Iso { get; set; }
        public int? LightSource { get; set; }
        public string Make { get; set; }
        public int? MeteringMode { get; set; }
        public string Model { get; set; }
        public int? Orientation { get; set; }
        public int? Saturation { get; set; }
        public int? SceneCaptureType { get; set; }
        public int? SceneType { get; set; }
        public int? SensingMethod { get; set; }
        public int? Sharpness { get; set; }
        
        // nikon - we must get these as strings, because we will have pictures that are not just for nikon
        //       - as such, we can't use the nikon lookup tables in all cases, so we have to manage these
        //       - as generic lookup tables instead
        public string AutoFocusAreaMode { get; set; }
        public string AutoFocusPoint { get; set; }
        public string ActiveDLighting { get; set; }
        public string Colorspace { get; set; }
        public double? ExposureDifference { get; set; }
        public string FlashColorFilter { get; set; }
        public string FlashCompensation { get; set; }
        public string FlashControlMode { get; set; }
        public string FlashExposureCompensation { get; set; }
        public double? FlashFocalLength { get; set; }
        public string FlashMode { get; set; }
        public string FlashSetting { get; set; }
        public string FlashSource { get; set; }
        public string FlashType { get; set; }
        public double? FocusDistance { get; set; }
        public string FocusMode { get; set; }
        public string FocusPosition { get; set; }
        public string HighIsoNoiseReduction { get; set; }
        public string HueAdjustment { get; set; }
        public string NoiseReduction { get; set; }
        public string PictureControlName { get; set; }
        public string PrimaryAFPoint { get; set; }
        public string VRMode { get; set; }
        public string VibrationReduction { get; set; }
        public string VignetteControl { get; set; }
        public string WhiteBalance { get; set; }
        
        // composite
        public double? Aperture { get; set; }
        public short? AutoFocus { get; set; }
        public string DepthOfField { get; set; }
        public string FieldOfView { get; set; }
        public double? HyperfocalDistance { get; set; }
        public string LensId { get; set; }
        public double? LightValue { get; set; }
        public double? ScaleFactor35Efl { get; set; }
        public string ShutterSpeed { get; set; }
    }
}
