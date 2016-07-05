using System.Collections.Generic;
using System.IO;
using System.Linq;
using SizePhotos.Optimizer;
using SizePhotos.Raw;


namespace SizePhotos.ResultWriters
{
    public class SqlUpdateWriter
        : IResultWriter
    {
        readonly string _file;
        StreamWriter _writer;
        List<ProcessingResult> _results = new List<ProcessingResult>();
        static readonly string[] _cols = new string[] 
        {
            // scaled images
            "xs_height",
            "xs_width",
            "xs_path",
            "sm_height",
            "sm_width",
            "sm_path",
            "md_height",
            "md_width",
            "md_path",
            "lg_height",
            "lg_width",
            "lg_path",
            "prt_height",
            "prt_width",
            "prt_path",
            "src_height",
            "src_width",
            "src_path",
            // exif
            "bits_per_sample",
            "compression_id",
            "contrast_id",
            "create_date",
            "digital_zoom_ratio",
            "exposure_compensation",
            "exposure_mode_id",
            "exposure_program_id",
            "exposure_time",
            "f_number",
            "flash_id",
            "focal_length",
            "focal_length_in_35_mm_format",
            "gain_control_id",
            "gps_altitude",
            "gps_altitude_ref_id",
            "gps_date_time_stamp",
            "gps_direction",
            "gps_direction_ref_id",
            "gps_latitude",
            "gps_latitude_ref_id",
            "gps_longitude",
            "gps_longitude_ref_id",
            "gps_measure_mode_id",
            "gps_satellites",
            "gps_status_id",
            "gps_version_id",
            "iso",
            "light_source_id",
            "make_id",
            "metering_mode_id",
            "model_id",
            "orientation_id",
            "saturation_id",
            "scene_capture_type_id",
            "scene_type_id",
            "sensing_method_id",
            "sharpness_id",
            // nikon
            "af_area_mode_id",
            "af_point_id",
            "active_d_lighting_id",
            "colorspace_id",
            "exposure_difference",
            "flash_color_filter_id",
            "flash_compensation",
            "flash_control_mode",
            "flash_exposure_compensation",
            "flash_focal_length",
            "flash_mode_id",
            "flash_setting_id",
            "flash_type_id",
            "focus_distance",
            "focus_mode_id",
            "focus_position",
            "high_iso_noise_reduction_id",
            "hue_adjustment_id",
            "noise_reduction_id",
            "picture_control_name_id",
            "primary_af_point",
            "vibration_reduction_id",
            "vignette_control_id",
            "vr_mode_id",
            "white_balance_id", 
            // composite
            "aperture",
            "auto_focus_id",
            "depth_of_field",
            "field_of_view",
            "hyperfocal_distance",
            "lens_id",
            "light_value",
            "scale_factor_35_efl",
            "shutter_speed",
            // image optimizations
            "raw_conversion_mode_id",
            "sigmoidal_contrast_adjustment",
            "saturation_adjustment",
            "compression_quality"
        };
        
        
        public SqlUpdateWriter(string outputFile)
        {
            _file = outputFile;
        }
        
        
        public void PreProcess(CategoryInfo category)
        {
            PrepareOutputStream();
        }
        
        
        public void PostProcess()
        {
            FinalizeOutputStream();
        }
        
        
        public void AddResult(ProcessingResult result)
        {
            _results.Add(result);
        }
        
        
        void PrepareOutputStream()
        {
            _writer = new StreamWriter(new FileStream(_file, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8192, FileOptions.None));
            
            _writer.WriteLine("DO");
            _writer.WriteLine("$$");
            _writer.WriteLine("BEGIN");
            _writer.WriteLine();
        }
        
        
        void FinalizeOutputStream()
        {
            WriteResultSql();
            
            _writer.WriteLine("END");
            _writer.WriteLine("$$");
            
            _writer.Flush();
            _writer.Close();
            _writer = null;
        }
        
        
        void WriteResultSql()
        {
            WriteLookups("photo.active_d_lighting", _results.Select(x => x.ExifData.ActiveDLighting).Distinct());
            WriteLookups("photo.af_area_mode", _results.Select(x => x.ExifData.AutoFocusAreaMode).Distinct());
            WriteLookups("photo.af_point", _results.Select(x => x.ExifData.AutoFocusPoint).Distinct());
            WriteLookups("photo.colorspace", _results.Select(x => x.ExifData.Colorspace).Distinct());
            WriteLookups("photo.flash_color_filter", _results.Select(x => x.ExifData.FlashColorFilter).Distinct());
            WriteLookups("photo.flash_mode", _results.Select(x => x.ExifData.FlashMode).Distinct());
            WriteLookups("photo.flash_setting", _results.Select(x => x.ExifData.FlashSetting).Distinct());
            WriteLookups("photo.flash_type", _results.Select(x => x.ExifData.FlashType).Distinct());
            WriteLookups("photo.focus_mode", _results.Select(x => x.ExifData.FocusMode).Distinct());
            WriteLookups("photo.high_iso_noise_reduction", _results.Select(x => x.ExifData.HighIsoNoiseReduction).Distinct());
            WriteLookups("photo.hue_adjustment", _results.Select(x => x.ExifData.HueAdjustment).Distinct());
            WriteLookups("photo.lens", _results.Select(x => x.ExifData.LensId).Distinct());
            WriteLookups("photo.make", _results.Select(x => x.ExifData.Make).Distinct());
            WriteLookups("photo.model", _results.Select(x => x.ExifData.Model).Distinct());
            WriteLookups("photo.noise_reduction", _results.Select(x => x.ExifData.NoiseReduction).Distinct());
            WriteLookups("photo.picture_control_name", _results.Select(x => x.ExifData.PictureControlName).Distinct());
            WriteLookups("photo.saturation", _results.Select(x => x.ExifData.Saturation).Distinct());
            WriteLookups("photo.vibration_reduction", _results.Select(x => x.ExifData.VibrationReduction).Distinct());
            WriteLookups("photo.vignette_control", _results.Select(x => x.ExifData.VignetteControl).Distinct());
            WriteLookups("photo.vr_mode", _results.Select(x => x.ExifData.VRMode).Distinct());
            WriteLookups("photo.white_balance", _results.Select(x => x.ExifData.WhiteBalance).Distinct());
            
            foreach(var result in _results)
            {
                var rawResult = result.RawConversionResult as RawConversionResult;
                var optResult = result.OptimizationResult as OptimizationResult;
                
                _writer.WriteLine($"UPDATE photo.category SET teaser_photo_width = {result.Xs.Width}, teaser_photo_height = {result.Xs.Height}, teaser_photo_path = {SqlHelper.SqlString(result.Xs.WebFilePath)} WHERE teaser_photo_path = {SqlHelper.SqlString(result.Xs.WebFilePath)};"); 
                
                var args = new string[] {
                    // scaled images
                    result.Xs.Height.ToString(),
                    result.Xs.Width.ToString(),
                    SqlHelper.SqlString(result.Xs.WebFilePath),
                    result.Sm.Height.ToString(),
                    result.Sm.Width.ToString(),
                    SqlHelper.SqlString(result.Sm.WebFilePath),
                    result.Md.Height.ToString(),
                    result.Md.Width.ToString(),
                    SqlHelper.SqlString(result.Md.WebFilePath),
                    result.Lg.Height.ToString(),
                    result.Lg.Width.ToString(),
                    SqlHelper.SqlString(result.Lg.WebFilePath),
                    result.Print.Height.ToString(),
                    result.Print.Width.ToString(),
                    SqlHelper.SqlString(result.Print.WebFilePath),
                    result.Source.Height.ToString(),
                    result.Source.Width.ToString(),
                    SqlHelper.SqlString(result.Source.WebFilePath),
                    // exif
                    SqlHelper.SqlNumber(result.ExifData.BitsPerSample),
                    SqlHelper.SqlNumber(result.ExifData.Compression),
                    SqlHelper.SqlString(result.ExifData.Contrast),
                    SqlHelper.SqlTimestamp(result.ExifData.CreateDate),
                    SqlHelper.SqlNumber(result.ExifData.DigitalZoomRatio),
                    SqlHelper.SqlString(result.ExifData.ExposureCompensation),
                    SqlHelper.SqlNumber(result.ExifData.ExposureMode),
                    SqlHelper.SqlNumber(result.ExifData.ExposureProgram),
                    SqlHelper.SqlString(result.ExifData.ExposureTime),
                    SqlHelper.SqlNumber(result.ExifData.FNumber),
                    SqlHelper.SqlNumber(result.ExifData.Flash),
                    SqlHelper.SqlNumber(result.ExifData.FocalLength),
                    SqlHelper.SqlNumber(result.ExifData.FocalLengthIn35mmFormat),
                    SqlHelper.SqlNumber(result.ExifData.GainControl),
                    SqlHelper.SqlNumber(result.ExifData.GpsAltitude),
                    SqlHelper.SqlNumber(result.ExifData.GpsAltitudeRef),
                    SqlHelper.SqlTimestamp(result.ExifData.GpsDateStamp),
                    SqlHelper.SqlNumber(result.ExifData.GpsDirection),
                    SqlHelper.SqlString(result.ExifData.GpsDirectionRef),
                    SqlHelper.SqlNumber(result.ExifData.GpsLatitude),
                    SqlHelper.SqlString(result.ExifData.GpsLatitudeRef),
                    SqlHelper.SqlNumber(result.ExifData.GpsLongitude),
                    SqlHelper.SqlString(result.ExifData.GpsLongitudeRef),
                    SqlHelper.SqlString(result.ExifData.GpsMeasureMode),
                    SqlHelper.SqlString(result.ExifData.GpsSatellites),
                    SqlHelper.SqlString(result.ExifData.GpsStatus),
                    SqlHelper.SqlString(result.ExifData.GpsVersionId),
                    SqlHelper.SqlNumber(result.ExifData.Iso),
                    SqlHelper.SqlNumber(result.ExifData.LightSource),
                    SqlHelper.SqlLookupId("photo.make", result.ExifData.Make),
                    SqlHelper.SqlNumber(result.ExifData.MeteringMode),
                    SqlHelper.SqlLookupId("photo.model", result.ExifData.Model),
                    SqlHelper.SqlNumber(result.ExifData.Orientation),
                    SqlHelper.SqlLookupId("photo.saturation", result.ExifData.Saturation),
                    SqlHelper.SqlNumber(result.ExifData.SceneCaptureType),
                    SqlHelper.SqlNumber(result.ExifData.SceneType),
                    SqlHelper.SqlNumber(result.ExifData.SensingMethod),
                    SqlHelper.SqlNumber(result.ExifData.Sharpness),
                    // nikon
                    SqlHelper.SqlLookupId("photo.af_area_mode", result.ExifData.AutoFocusAreaMode),
                    SqlHelper.SqlLookupId("photo.af_point", result.ExifData.AutoFocusPoint),
                    SqlHelper.SqlLookupId("photo.active_d_lighting", result.ExifData.ActiveDLighting),
                    SqlHelper.SqlLookupId("photo.colorspace", result.ExifData.Colorspace),
                    SqlHelper.SqlNumber(result.ExifData.ExposureDifference),
                    SqlHelper.SqlLookupId("photo.flash_color_filter", result.ExifData.FlashColorFilter),
                    SqlHelper.SqlString(result.ExifData.FlashCompensation),
                    SqlHelper.SqlNumber(result.ExifData.FlashControlMode),
                    SqlHelper.SqlString(result.ExifData.FlashExposureCompensation),
                    SqlHelper.SqlNumber(result.ExifData.FlashFocalLength),
                    SqlHelper.SqlLookupId("photo.flash_mode", result.ExifData.FlashMode),
                    SqlHelper.SqlLookupId("photo.flash_setting", result.ExifData.FlashSetting),
                    SqlHelper.SqlLookupId("photo.flash_type", result.ExifData.FlashType),
                    SqlHelper.SqlNumber(result.ExifData.FocusDistance),
                    SqlHelper.SqlLookupId("photo.focus_mode", result.ExifData.FocusMode),
                    SqlHelper.SqlNumber(result.ExifData.FocusPosition),
                    SqlHelper.SqlLookupId("photo.high_iso_noise_reduction", result.ExifData.HighIsoNoiseReduction),
                    SqlHelper.SqlLookupId("photo.hue_adjustment", result.ExifData.HueAdjustment),
                    SqlHelper.SqlLookupId("photo.noise_reduction", result.ExifData.NoiseReduction),
                    SqlHelper.SqlLookupId("photo.picture_control_name", result.ExifData.PictureControlName),
                    SqlHelper.SqlString(result.ExifData.PrimaryAFPoint),
                    SqlHelper.SqlLookupId("photo.vibration_reduction", result.ExifData.VibrationReduction),
                    SqlHelper.SqlLookupId("photo.vignette_control", result.ExifData.VignetteControl),
                    SqlHelper.SqlLookupId("photo.vr_mode", result.ExifData.VRMode),
                    SqlHelper.SqlLookupId("photo.white_balance", result.ExifData.WhiteBalance),
                    // composite
                    SqlHelper.SqlNumber(result.ExifData.Aperture),
                    SqlHelper.SqlNumber(result.ExifData.AutoFocus),
                    SqlHelper.SqlString(result.ExifData.DepthOfField),
                    SqlHelper.SqlString(result.ExifData.FieldOfView),
                    SqlHelper.SqlNumber(result.ExifData.HyperfocalDistance),
                    SqlHelper.SqlLookupId("photo.lens", result.ExifData.LensId),
                    SqlHelper.SqlNumber(result.ExifData.LightValue),
                    SqlHelper.SqlNumber(result.ExifData.ScaleFactor35Efl),
                    SqlHelper.SqlString(result.ExifData.ShutterSpeed),
                    // image optimizations
                    SqlHelper.SqlNumber((short?)rawResult?.Mode),
                    SqlHelper.SqlNumber(optResult?.SigmoidalOptimization),
                    SqlHelper.SqlNumber(optResult?.SaturationOptimization),
                    SqlHelper.SqlNumber(result.CompressionQuality)
                };
                
                string[] sets = new string[_cols.Length];
                 
                for(int i = 0; i < _cols.Length; i++)
                {
                    sets[i] = $"{_cols[i]} = {args[i]}";
                }
                
                _writer.WriteLine($"UPDATE photo.photo SET {string.Join(", ", sets)} WHERE lg_path = {SqlHelper.SqlString(result.Lg.WebFilePath)};");
                _writer.WriteLine();
            }
        }
        
        
        void WriteLookups(string table, IEnumerable<string> values)
        {
            foreach(string val in values)
            {
                var lookup = SqlHelper.SqlCreateLookup(table, val);
                
                if(!string.IsNullOrWhiteSpace(lookup))
                {
                    _writer.WriteLine(lookup);
                }
            }
        }
    }
}
