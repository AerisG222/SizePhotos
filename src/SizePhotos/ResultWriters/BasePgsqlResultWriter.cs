using System.Collections.Generic;
using System.Linq;
using SizePhotos.Exif;

namespace SizePhotos.ResultWriters;

public abstract class BasePgsqlResultWriter
    : BaseResultWriter
{
    protected void WriteHeader()
    {
        _writer.WriteLine("DO");
        _writer.WriteLine("$$");
        _writer.WriteLine("BEGIN");
        _writer.WriteLine();
    }

    protected void WriteFooter()
    {
        _writer.WriteLine("END");
        _writer.WriteLine("$$");

        _writer.Flush();
        _writer.Dispose();
        _writer = null;
    }

    protected void WriteLookups()
    {
        var exifDataList = _results
            .Select(x => x.GetExifResult())
            .ToList();

        WriteLookups("photo.active_d_lighting", exifDataList.Select(x => x.ExifData.ActiveDLighting).Distinct());
        WriteLookups("photo.af_area_mode", exifDataList.Select(x => x.ExifData.AutoFocusAreaMode).Distinct());
        WriteLookups("photo.af_point", exifDataList.Select(x => x.ExifData.AutoFocusPoint).Distinct());
        WriteLookups("photo.colorspace", exifDataList.Select(x => x.ExifData.Colorspace).Distinct());
        WriteLookups("photo.flash_color_filter", exifDataList.Select(x => x.ExifData.FlashColorFilter).Distinct());
        WriteLookups("photo.flash_mode", exifDataList.Select(x => x.ExifData.FlashMode).Distinct());
        WriteLookups("photo.flash_setting", exifDataList.Select(x => x.ExifData.FlashSetting).Distinct());
        WriteLookups("photo.flash_type", exifDataList.Select(x => x.ExifData.FlashType).Distinct());
        WriteLookups("photo.focus_mode", exifDataList.Select(x => x.ExifData.FocusMode).Distinct());
        WriteLookups("photo.high_iso_noise_reduction", exifDataList.Select(x => x.ExifData.HighIsoNoiseReduction).Distinct());
        WriteLookups("photo.hue_adjustment", exifDataList.Select(x => x.ExifData.HueAdjustment).Distinct());
        WriteLookups("photo.lens", exifDataList.Select(x => x.ExifData.LensId).Distinct());
        WriteLookups("photo.make", exifDataList.Select(x => x.ExifData.Make).Distinct());
        WriteLookups("photo.model", exifDataList.Select(x => x.ExifData.Model).Distinct());
        WriteLookups("photo.noise_reduction", exifDataList.Select(x => x.ExifData.NoiseReduction).Distinct());
        WriteLookups("photo.picture_control_name", exifDataList.Select(x => x.ExifData.PictureControlName).Distinct());
        WriteLookups("photo.saturation", exifDataList.Select(x => x.ExifData.Saturation).Distinct());
        WriteLookups("photo.vibration_reduction", exifDataList.Select(x => x.ExifData.VibrationReduction).Distinct());
        WriteLookups("photo.vignette_control", exifDataList.Select(x => x.ExifData.VignetteControl).Distinct());
        WriteLookups("photo.vr_mode", exifDataList.Select(x => x.ExifData.VRMode).Distinct());
        WriteLookups("photo.white_balance", exifDataList.Select(x => x.ExifData.WhiteBalance).Distinct());

        WriteLookups("photo.gps_altitude_ref", exifDataList.Select(x => x.ExifData.GpsAltitudeRef).Distinct());
    }

    void WriteLookups(string table, IEnumerable<string> values)
    {
        foreach (string val in values)
        {
            var lookup = SqlHelper.SqlCreateLookup(table, val);

            if (!string.IsNullOrWhiteSpace(lookup))
            {
                _writer.WriteLine(lookup);
            }
        }
    }
}
