using System.IO;
using System.Linq;
using SizePhotos.Exif;
using SizePhotos.Minification;
using SizePhotos.PhotoReaders;
using SizePhotos.PhotoWriters;


namespace SizePhotos.ResultWriters
{
    public class PgsqlInsertResultWriter
        : BasePgsqlResultWriter
    {
        readonly string _file;
        CategoryInfo _category;
        static readonly string[] _cols = new string[] 
        {
            "category_id",
            "is_private",
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
            "shutter_speed"
        };
        
        
        public PgsqlInsertResultWriter(string outputFile)
        {
            _file = outputFile;
        }
        
        
        public override void PreProcess(CategoryInfo category)
        {
            _category = category;
            PrepareOutputStream();
        }
        
        
        public override void PostProcess()
        {
            FinalizeOutputStream();
        }
        
        
        public override void AddResult(ProcessingContext ctx)
        {
            _results.Add(ctx);
        }
        
        
        void PrepareOutputStream()
        {
            _writer = new StreamWriter(new FileStream(_file, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8192, FileOptions.None));
            
            WriteHeader();
        }
        
        
        void FinalizeOutputStream()
        {
            WriteResultSql();
            WriteFooter();
        }
        
        
        void WriteResultSql()
        {
            WriteCategoryCreate();

            WriteLookups();

            foreach(var result in _results)
            {
                var exifData = result.GetExifResult()?.ExifData;
                
                var xs = result.GetPhotoWriterResult("xs");
                var sm = result.GetPhotoWriterResult("sm");
                var md = result.GetPhotoWriterResult("md");
                var lg = result.GetPhotoWriterResult("lg");
                var prt = result.GetPhotoWriterResult("prt");
                var src = result.GetSuccessfulPhotoReaderResult();

                var values = new string[] {
                    "(SELECT currval('photo.category_id_seq'))",
                    _category.IsPrivate.ToString(),
                    // scaled images
                    xs.Height.ToString(),
                    xs.Width.ToString(),
                    SqlHelper.SqlString(xs.Url),
                    sm.Height.ToString(),
                    sm.Width.ToString(),
                    SqlHelper.SqlString(sm.Url),
                    md.Height.ToString(),
                    md.Width.ToString(),
                    SqlHelper.SqlString(md.Url),
                    lg.Height.ToString(),
                    lg.Width.ToString(),
                    SqlHelper.SqlString(lg.Url),
                    prt.Height.ToString(),
                    prt.Width.ToString(),
                    SqlHelper.SqlString(prt.Url),
                    src.Height.ToString(),
                    src.Width.ToString(),
                    SqlHelper.SqlString(src.Url),
                    // exif
                    SqlHelper.SqlNumber(exifData?.BitsPerSample),
                    SqlHelper.SqlNumber(exifData?.Compression),
                    SqlHelper.SqlString(exifData?.Contrast),
                    SqlHelper.SqlTimestamp(exifData?.CreateDate),
                    SqlHelper.SqlNumber(exifData?.DigitalZoomRatio),
                    SqlHelper.SqlString(exifData?.ExposureCompensation),
                    SqlHelper.SqlNumber(exifData?.ExposureMode),
                    SqlHelper.SqlNumber(exifData?.ExposureProgram),
                    SqlHelper.SqlString(exifData?.ExposureTime),
                    SqlHelper.SqlNumber(exifData?.FNumber),
                    SqlHelper.SqlNumber(exifData?.Flash),
                    SqlHelper.SqlNumber(exifData?.FocalLength),
                    SqlHelper.SqlNumber(exifData?.FocalLengthIn35mmFormat),
                    SqlHelper.SqlNumber(exifData?.GainControl),
                    SqlHelper.SqlNumber(exifData?.GpsAltitude),
                    SqlHelper.SqlNumber(exifData?.GpsAltitudeRef),
                    SqlHelper.SqlTimestamp(exifData?.GpsDateStamp),
                    SqlHelper.SqlNumber(exifData?.GpsDirection),
                    SqlHelper.SqlString(exifData?.GpsDirectionRef),
                    SqlHelper.SqlNumber(exifData?.GpsLatitude),
                    SqlHelper.SqlString(exifData?.GpsLatitudeRef),
                    SqlHelper.SqlNumber(exifData?.GpsLongitude),
                    SqlHelper.SqlString(exifData?.GpsLongitudeRef),
                    SqlHelper.SqlString(exifData?.GpsMeasureMode),
                    SqlHelper.SqlString(exifData?.GpsSatellites),
                    SqlHelper.SqlString(exifData?.GpsStatus),
                    SqlHelper.SqlString(exifData?.GpsVersionId),
                    SqlHelper.SqlNumber(exifData?.Iso),
                    SqlHelper.SqlNumber(exifData?.LightSource),
                    SqlHelper.SqlLookupId("photo.make", exifData?.Make),
                    SqlHelper.SqlNumber(exifData?.MeteringMode),
                    SqlHelper.SqlLookupId("photo.model", exifData?.Model),
                    SqlHelper.SqlNumber(exifData?.Orientation),
                    SqlHelper.SqlLookupId("photo.saturation", exifData?.Saturation),
                    SqlHelper.SqlNumber(exifData?.SceneCaptureType),
                    SqlHelper.SqlNumber(exifData?.SceneType),
                    SqlHelper.SqlNumber(exifData?.SensingMethod),
                    SqlHelper.SqlNumber(exifData?.Sharpness),
                    // nikon
                    SqlHelper.SqlLookupId("photo.af_area_mode", exifData?.AutoFocusAreaMode),
                    SqlHelper.SqlLookupId("photo.af_point", exifData?.AutoFocusPoint),
                    SqlHelper.SqlLookupId("photo.active_d_lighting", exifData?.ActiveDLighting),
                    SqlHelper.SqlLookupId("photo.colorspace", exifData?.Colorspace),
                    SqlHelper.SqlNumber(exifData?.ExposureDifference),
                    SqlHelper.SqlLookupId("photo.flash_color_filter", exifData?.FlashColorFilter),
                    SqlHelper.SqlString(exifData?.FlashCompensation),
                    SqlHelper.SqlNumber(exifData?.FlashControlMode),
                    SqlHelper.SqlString(exifData?.FlashExposureCompensation),
                    SqlHelper.SqlNumber(exifData?.FlashFocalLength),
                    SqlHelper.SqlLookupId("photo.flash_mode", exifData?.FlashMode),
                    SqlHelper.SqlLookupId("photo.flash_setting", exifData?.FlashSetting),
                    SqlHelper.SqlLookupId("photo.flash_type", exifData?.FlashType),
                    SqlHelper.SqlNumber(exifData?.FocusDistance),
                    SqlHelper.SqlLookupId("photo.focus_mode", exifData?.FocusMode),
                    SqlHelper.SqlNumber(exifData?.FocusPosition),
                    SqlHelper.SqlLookupId("photo.high_iso_noise_reduction", exifData?.HighIsoNoiseReduction),
                    SqlHelper.SqlLookupId("photo.hue_adjustment", exifData?.HueAdjustment),
                    SqlHelper.SqlLookupId("photo.noise_reduction", exifData?.NoiseReduction),
                    SqlHelper.SqlLookupId("photo.picture_control_name", exifData?.PictureControlName),
                    SqlHelper.SqlString(exifData?.PrimaryAFPoint),
                    SqlHelper.SqlLookupId("photo.vibration_reduction", exifData?.VibrationReduction),
                    SqlHelper.SqlLookupId("photo.vignette_control", exifData?.VignetteControl),
                    SqlHelper.SqlLookupId("photo.vr_mode", exifData?.VRMode),
                    SqlHelper.SqlLookupId("photo.white_balance", exifData?.WhiteBalance),
                    // composite
                    SqlHelper.SqlNumber(exifData?.Aperture),
                    SqlHelper.SqlNumber(exifData?.AutoFocus),
                    SqlHelper.SqlString(exifData?.DepthOfField),
                    SqlHelper.SqlString(exifData?.FieldOfView),
                    SqlHelper.SqlNumber(exifData?.HyperfocalDistance),
                    SqlHelper.SqlLookupId("photo.lens", exifData?.LensId),
                    SqlHelper.SqlNumber(exifData?.LightValue),
                    SqlHelper.SqlNumber(exifData?.ScaleFactor35Efl),
                    SqlHelper.SqlString(exifData?.ShutterSpeed)
                };
                
                _writer.WriteLine($"INSERT INTO photo.photo ({string.Join(", ", _cols)}) VALUES ({string.Join(", ", values)});");
            }
            
            _writer.WriteLine();
        }
        
        
        void WriteCategoryCreate()
        {
            var result = _results.First();
            var xs = result.GetPhotoWriterResult("xs");

            _writer.WriteLine(string.Concat("    INSERT INTO photo.category (name, year, is_private, teaser_photo_width, teaser_photo_height, teaser_photo_path) ",
                                                "VALUES (", SqlHelper.SqlString(_category.Name), ", ", _category.Year, ", ", _category.IsPrivate, ", ", xs.Width, ", ", xs.Height, ", ", SqlHelper.SqlString(xs.Url), ");"));

            _writer.WriteLine();
        }
    }
}
