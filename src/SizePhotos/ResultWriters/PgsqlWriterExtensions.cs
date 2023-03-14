using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SizePhotos.ResultWriters;

public static class BasePgsqlResultWriter
{
    public static void WriteHeader(this StreamWriter writer)
    {
        writer.WriteLine("DO");
        writer.WriteLine("$$");
        writer.WriteLine("BEGIN");
        writer.WriteLine();
    }

    public static void WriteFooter(this StreamWriter writer)
    {
        writer.WriteLine("END");
        writer.WriteLine("$$");
    }

    public static void WriteLookups(this StreamWriter writer, IEnumerable<ProcessedPhoto> photos)
    {
        writer.WriteLookup("photo.active_d_lighting", photos.Select(x => x.ExifData.ActiveDLighting).Distinct());
        writer.WriteLookup("photo.af_area_mode", photos.Select(x => x.ExifData.AutoFocusAreaMode).Distinct());
        writer.WriteLookup("photo.af_point", photos.Select(x => x.ExifData.AutoFocusPoint).Distinct());
        writer.WriteLookup("photo.colorspace", photos.Select(x => x.ExifData.Colorspace).Distinct());
        writer.WriteLookup("photo.flash_color_filter", photos.Select(x => x.ExifData.FlashColorFilter).Distinct());
        writer.WriteLookup("photo.flash_mode", photos.Select(x => x.ExifData.FlashMode).Distinct());
        writer.WriteLookup("photo.flash_setting", photos.Select(x => x.ExifData.FlashSetting).Distinct());
        writer.WriteLookup("photo.flash_type", photos.Select(x => x.ExifData.FlashType).Distinct());
        writer.WriteLookup("photo.focus_mode", photos.Select(x => x.ExifData.FocusMode).Distinct());
        writer.WriteLookup("photo.high_iso_noise_reduction", photos.Select(x => x.ExifData.HighIsoNoiseReduction).Distinct());
        writer.WriteLookup("photo.hue_adjustment", photos.Select(x => x.ExifData.HueAdjustment).Distinct());
        writer.WriteLookup("photo.lens", photos.Select(x => x.ExifData.LensId).Distinct());
        writer.WriteLookup("photo.make", photos.Select(x => x.ExifData.Make).Distinct());
        writer.WriteLookup("photo.model", photos.Select(x => x.ExifData.Model).Distinct());
        writer.WriteLookup("photo.noise_reduction", photos.Select(x => x.ExifData.NoiseReduction).Distinct());
        writer.WriteLookup("photo.picture_control_name", photos.Select(x => x.ExifData.PictureControlName).Distinct());
        writer.WriteLookup("photo.saturation", photos.Select(x => x.ExifData.Saturation).Distinct());
        writer.WriteLookup("photo.vibration_reduction", photos.Select(x => x.ExifData.VibrationReduction).Distinct());
        writer.WriteLookup("photo.vignette_control", photos.Select(x => x.ExifData.VignetteControl).Distinct());
        writer.WriteLookup("photo.vr_mode", photos.Select(x => x.ExifData.VRMode).Distinct());
        writer.WriteLookup("photo.white_balance", photos.Select(x => x.ExifData.WhiteBalance).Distinct());

        writer.WriteLookup("photo.gps_altitude_ref", photos.Select(x => x.ExifData.GpsAltitudeRef).Distinct());
    }

    public static void WriteLookup(this StreamWriter writer, string table, IEnumerable<string> values)
    {
        foreach (string val in values)
        {
            var lookup = SqlHelper.SqlCreateLookup(table, val);

            if (!string.IsNullOrWhiteSpace(lookup))
            {
                writer.WriteLine(lookup);
            }
        }
    }
}
