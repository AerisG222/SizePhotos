using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SizePhotos.ResultWriters;

public class PgsqlInsertResultWriter
    : IResultWriter
{
    static readonly string[] _cols = new string[]
    {
            "category_id",
            // scaled images
            "xs_height",
            "xs_width",
            "xs_size",
            "xs_path",
            "xs_sq_height",
            "xs_sq_width",
            "xs_sq_size",
            "xs_sq_path",
            "sm_height",
            "sm_width",
            "sm_size",
            "sm_path",
            "md_height",
            "md_width",
            "md_size",
            "md_path",
            "lg_height",
            "lg_width",
            "lg_size",
            "lg_path",
            "prt_height",
            "prt_width",
            "prt_size",
            "prt_path",
            "src_height",
            "src_width",
            "src_size",
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

    readonly SizePhotoOptions _opts;

    public PgsqlInsertResultWriter(SizePhotoOptions opts)
    {
        _opts = opts ?? throw new ArgumentNullException(nameof(opts));
    }

    public void WriteOutput(string outputFile, CategoryInfo category, IEnumerable<ProcessedPhoto> photos)
    {
        if(string.IsNullOrWhiteSpace(outputFile))
        {
            throw new ArgumentNullException(nameof(outputFile));
        }

        using var writer = new StreamWriter(new FileStream(outputFile, FileMode.Create));

        writer.WriteHeader();

        WriteCategoryCreate(writer, category, photos);
        writer.WriteLookups(photos);
        WriteResultSql(writer, category, photos);
        WriteCategoryUpdate(writer);

        writer.WriteFooter();

        writer.Flush();
    }

    void WriteResultSql(StreamWriter writer, CategoryInfo category, IEnumerable<ProcessedPhoto> photos)
    {
        foreach (var photo in photos)
        {
            var values = new string[] {
                    "(SELECT currval('photo.category_id_seq'))",
                    // scaled images
                    photo.Xs.Height.ToString(),
                    photo.Xs.Width.ToString(),
                    photo.Xs.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.Xs.OutputFile)),
                    photo.XsSq.Height.ToString(),
                    photo.XsSq.Width.ToString(),
                    photo.XsSq.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.XsSq.OutputFile)),
                    photo.Sm.Height.ToString(),
                    photo.Sm.Width.ToString(),
                    photo.Sm.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.Sm.OutputFile)),
                    photo.Md.Height.ToString(),
                    photo.Md.Width.ToString(),
                    photo.Md.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.Md.OutputFile)),
                    photo.Lg.Height.ToString(),
                    photo.Lg.Width.ToString(),
                    photo.Lg.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.Lg.OutputFile)),
                    photo.Prt.Height.ToString(),
                    photo.Prt.Width.ToString(),
                    photo.Prt.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.Prt.OutputFile)),
                    photo.Src.Height.ToString(),
                    photo.Src.Width.ToString(),
                    photo.Src.SizeInBytes.ToString(),
                    SqlHelper.SqlString(BuildUrl(category, photo.Src.OutputFile)),
                    // exif
                    SqlHelper.SqlNumber(photo.ExifData?.BitsPerSample),
                    SqlHelper.SqlNumber(photo.ExifData?.Compression),
                    SqlHelper.SqlString(photo.ExifData?.Contrast),
                    SqlHelper.SqlTimestamp(photo.ExifData?.CreateDate),
                    SqlHelper.SqlNumber(photo.ExifData?.DigitalZoomRatio),
                    SqlHelper.SqlString(photo.ExifData?.ExposureCompensation),
                    SqlHelper.SqlNumber(photo.ExifData?.ExposureMode),
                    SqlHelper.SqlNumber(photo.ExifData?.ExposureProgram),
                    SqlHelper.SqlString(photo.ExifData?.ExposureTime),
                    SqlHelper.SqlNumber(photo.ExifData?.FNumber),
                    SqlHelper.SqlNumber(photo.ExifData?.Flash),
                    SqlHelper.SqlNumber(photo.ExifData?.FocalLength),
                    SqlHelper.SqlNumber(photo.ExifData?.FocalLengthIn35mmFormat),
                    SqlHelper.SqlNumber(photo.ExifData?.GainControl),
                    SqlHelper.SqlNumber(photo.ExifData?.GpsAltitude),
                    SqlHelper.SqlLookupId("photo.gps_altitude_ref", photo.ExifData?.GpsAltitudeRef),
                    SqlHelper.SqlTimestamp(photo.ExifData?.GpsDateStamp),
                    SqlHelper.SqlNumber(photo.ExifData?.GpsDirection),
                    SqlHelper.SqlString(photo.ExifData?.GpsDirectionRef),
                    SqlHelper.SqlNumber(photo.ExifData?.GpsLatitude),
                    SqlHelper.SqlString(photo.ExifData?.GpsLatitudeRef),
                    SqlHelper.SqlNumber(photo.ExifData?.GpsLongitude),
                    SqlHelper.SqlString(photo.ExifData?.GpsLongitudeRef),
                    SqlHelper.SqlString(photo.ExifData?.GpsMeasureMode),
                    SqlHelper.SqlString(photo.ExifData?.GpsSatellites),
                    SqlHelper.SqlString(photo.ExifData?.GpsStatus),
                    SqlHelper.SqlString(photo.ExifData?.GpsVersionId),
                    SqlHelper.SqlNumber(photo.ExifData?.Iso),
                    SqlHelper.SqlNumber(photo.ExifData?.LightSource),
                    SqlHelper.SqlLookupId("photo.make", photo.ExifData?.Make),
                    SqlHelper.SqlNumber(photo.ExifData?.MeteringMode),
                    SqlHelper.SqlLookupId("photo.model", photo.ExifData?.Model),
                    SqlHelper.SqlNumber(photo.ExifData?.Orientation),
                    SqlHelper.SqlLookupId("photo.saturation", photo.ExifData?.Saturation),
                    SqlHelper.SqlNumber(photo.ExifData?.SceneCaptureType),
                    SqlHelper.SqlNumber(photo.ExifData?.SceneType),
                    SqlHelper.SqlNumber(photo.ExifData?.SensingMethod),
                    SqlHelper.SqlNumber(photo.ExifData?.Sharpness),
                    // nikon
                    SqlHelper.SqlLookupId("photo.af_area_mode", photo.ExifData?.AutoFocusAreaMode),
                    SqlHelper.SqlLookupId("photo.af_point", photo.ExifData?.AutoFocusPoint),
                    SqlHelper.SqlLookupId("photo.active_d_lighting", photo.ExifData?.ActiveDLighting),
                    SqlHelper.SqlLookupId("photo.colorspace", photo.ExifData?.Colorspace),
                    SqlHelper.SqlNumber(photo.ExifData?.ExposureDifference),
                    SqlHelper.SqlLookupId("photo.flash_color_filter", photo.ExifData?.FlashColorFilter),
                    SqlHelper.SqlString(photo.ExifData?.FlashCompensation),
                    SqlHelper.SqlNumber(photo.ExifData?.FlashControlMode),
                    SqlHelper.SqlString(photo.ExifData?.FlashExposureCompensation),
                    SqlHelper.SqlNumber(photo.ExifData?.FlashFocalLength),
                    SqlHelper.SqlLookupId("photo.flash_mode", photo.ExifData?.FlashMode),
                    SqlHelper.SqlLookupId("photo.flash_setting", photo.ExifData?.FlashSetting),
                    SqlHelper.SqlLookupId("photo.flash_type", photo.ExifData?.FlashType),
                    SqlHelper.SqlNumber(photo.ExifData?.FocusDistance),
                    SqlHelper.SqlLookupId("photo.focus_mode", photo.ExifData?.FocusMode),
                    SqlHelper.SqlNumber(photo.ExifData?.FocusPosition),
                    SqlHelper.SqlLookupId("photo.high_iso_noise_reduction", photo.ExifData?.HighIsoNoiseReduction),
                    SqlHelper.SqlLookupId("photo.hue_adjustment", photo.ExifData?.HueAdjustment),
                    SqlHelper.SqlLookupId("photo.noise_reduction", photo.ExifData?.NoiseReduction),
                    SqlHelper.SqlLookupId("photo.picture_control_name", photo.ExifData?.PictureControlName),
                    SqlHelper.SqlString(photo.ExifData?.PrimaryAFPoint),
                    SqlHelper.SqlLookupId("photo.vibration_reduction", photo.ExifData?.VibrationReduction),
                    SqlHelper.SqlLookupId("photo.vignette_control", photo.ExifData?.VignetteControl),
                    SqlHelper.SqlLookupId("photo.vr_mode", photo.ExifData?.VRMode),
                    SqlHelper.SqlLookupId("photo.white_balance", photo.ExifData?.WhiteBalance),
                    // composite
                    SqlHelper.SqlNumber(photo.ExifData?.Aperture),
                    SqlHelper.SqlNumber(photo.ExifData?.AutoFocus),
                    SqlHelper.SqlString(photo.ExifData?.DepthOfField),
                    SqlHelper.SqlString(photo.ExifData?.FieldOfView),
                    SqlHelper.SqlNumber(photo.ExifData?.HyperfocalDistance),
                    SqlHelper.SqlLookupId("photo.lens", photo.ExifData?.LensId),
                    SqlHelper.SqlNumber(photo.ExifData?.LightValue),
                    SqlHelper.SqlNumber(photo.ExifData?.ScaleFactor35Efl),
                    SqlHelper.SqlString(photo.ExifData?.ShutterSpeed)
                };

            writer.WriteLine($"INSERT INTO photo.photo ({string.Join(", ", _cols)}) VALUES ({string.Join(", ", values)});");
        }

        writer.WriteLine();
    }

    void WriteCategoryCreate(StreamWriter writer, CategoryInfo category, IEnumerable<ProcessedPhoto> photos)
    {
        var photo = photos.First();

        writer.WriteLine(
            $"INSERT INTO photo.category (name, year, teaser_photo_width, teaser_photo_height, teaser_photo_size, teaser_photo_path, teaser_photo_sq_width, teaser_photo_sq_height, teaser_photo_sq_size, teaser_photo_sq_path) " +
            $"  VALUES (" +
            $"    {SqlHelper.SqlString(category.Name)}, " +
            $"    {category.Year}, " +
            $"    {photo.Xs.Width}, " +
            $"    {photo.Xs.Height}, " +
            $"    {photo.Xs.SizeInBytes}, " +
            $"    {SqlHelper.SqlString(BuildUrl(category, photo.Xs.OutputFile))}, " +
            $"    {photo.XsSq.Width}, " +
            $"    {photo.XsSq.Height}, " +
            $"    {photo.XsSq.SizeInBytes}, " +
            $"    {SqlHelper.SqlString(BuildUrl(category, photo.XsSq.OutputFile))});");

        foreach (var role in category.AllowedRoles)
        {
            writer.WriteLine(
                $"INSERT INTO photo.category_role (category_id, role_id)" +
                $"  VALUES (" +
                $"    (SELECT currval('photo.category_id_seq'))," +
                $"    (SELECT id FROM maw.role WHERE name = {SqlHelper.SqlString(role)})" +
                $"  );"
            );
        }

        writer.WriteLine();
    }

    void WriteCategoryUpdate(StreamWriter writer)
    {
        writer.WriteLine(
            "UPDATE photo.category c " +
            "   SET photo_count = (SELECT COUNT(1) FROM photo.photo WHERE category_id = c.id), " +
            "       create_date = (SELECT create_date FROM photo.photo WHERE id = (SELECT MIN(id) FROM photo.photo where category_id = c.id AND create_date IS NOT NULL)), " +
            "       gps_latitude = (SELECT gps_latitude FROM photo.photo WHERE id = (SELECT MIN(id) FROM photo.photo WHERE category_id = c.id AND gps_latitude IS NOT NULL)), " +
            "       gps_latitude_ref_id = (SELECT gps_latitude_ref_id FROM photo.photo WHERE id = (SELECT MIN(id) FROM photo.photo WHERE category_id = c.id AND gps_latitude IS NOT NULL)), " +
            "       gps_longitude = (SELECT gps_longitude FROM photo.photo WHERE id = (SELECT MIN(id) FROM photo.photo WHERE category_id = c.id AND gps_latitude IS NOT NULL)), " +
            "       gps_longitude_ref_id = (SELECT gps_longitude_ref_id FROM photo.photo WHERE id = (SELECT MIN(id) FROM photo.photo WHERE category_id = c.id AND gps_latitude IS NOT NULL)), " +
            "       total_size_xs = (SELECT SUM(xs_size) FROM photo.photo WHERE category_id = c.id), " +
            "       total_size_xs_sq = (SELECT SUM(xs_sq_size) FROM photo.photo WHERE category_id = c.id), " +
            "       total_size_sm = (SELECT SUM(sm_size) FROM photo.photo WHERE category_id = c.id), " +
            "       total_size_md = (SELECT SUM(md_size) FROM photo.photo WHERE category_id = c.id), " +
            "       total_size_lg = (SELECT SUM(lg_size) FROM photo.photo WHERE category_id = c.id), " +
            "       total_size_prt = (SELECT SUM(prt_size) FROM photo.photo WHERE category_id = c.id), " +
            "       total_size_src = (SELECT SUM(src_size) FROM photo.photo WHERE category_id = c.id), " +
            "       teaser_photo_size = (SELECT xs_size FROM photo.photo WHERE category_id = c.id AND xs_path = c.teaser_photo_path), " +
            "       teaser_photo_sq_height = (SELECT xs_sq_height FROM photo.photo WHERE category_id = c.id AND xs_path = c.teaser_photo_path), " +
            "       teaser_photo_sq_width = (SELECT xs_sq_width FROM photo.photo WHERE category_id = c.id AND xs_path = c.teaser_photo_path), " +
            "       teaser_photo_sq_path = (SELECT xs_sq_path FROM photo.photo WHERE category_id = c.id AND xs_path = c.teaser_photo_path), " +
            "       teaser_photo_sq_size = (SELECT xs_sq_size FROM photo.photo WHERE category_id = c.id AND xs_path = c.teaser_photo_path) " +
            " WHERE c.id = (SELECT currval('photo.category_id_seq'));"
        );

        writer.WriteLine();
    }

    string BuildUrl(CategoryInfo category, string file)
    {
        var rootParts = _opts.WebPhotoRoot.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var root = $"/{string.Join('/', rootParts)}";
        var fileParts = file.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        if(fileParts.Length < 3)
        {
            throw new InvalidOperationException("should have at least 3 components of the image name!");
        }

        var filename = fileParts[^1];
        var sizeDir = fileParts[^2];
        var categoryDir = fileParts[^3];

        return $"{root}/{category.Year}/{categoryDir}/{sizeDir}/{filename}";
    }
}
