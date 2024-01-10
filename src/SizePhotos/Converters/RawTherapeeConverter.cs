using System;
using System.IO;
using System.Threading.Tasks;
using NRawTherapee;
using NRawTherapee.OutputFormat;
using SizePhotos.Exif;

namespace SizePhotos.Converters;

public class RawTherapeeConverter
{
    const string CURVE_PREFIX_AUTO = "auto_matched_curve";
    const string CURVE_PREFIX_FILM = "standard_film_curve";

    readonly SizePhotoOptions _opts;

    public RawTherapeeConverter(SizePhotoOptions opts)
    {
        _opts = opts ?? throw new ArgumentNullException(nameof(opts));
    }

    public Task ConvertAsync(string sourceFile, string destFile, ExifData exifData)
    {
        var opts = new Options
        {
            RawTherapeePath = "rawtherapee-cli",
            OutputFormat = new TiffOutputFormat(),
            OutputFile = destFile
        };

        if (IsRawFile(sourceFile))
        {
            if(_opts.Profile == ProcessingProfile.Neutral)
            {
                opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, "neutral.pp3"));
            }
            else
            {
                var prefix = _opts.Profile == ProcessingProfile.Film ? CURVE_PREFIX_FILM : CURVE_PREFIX_AUTO;

                if(exifData?.Iso == null)
                {
                    opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, $"{prefix}_iso_low.pp3"));
                }
                else
                {
                    if(exifData.Iso < 3200) {
                        opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, $"{prefix}_iso_low.pp3"));
                    }
                    else if(exifData.Iso < 6400) {
                        opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, $"{prefix}_iso_medium.pp3"));
                    }
                    else {
                        opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, $"{prefix}_iso_high.pp3"));
                    }
                }
            }

            opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, "contrast_saturation.pp3"));
        }
        else
        {
            opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, "neutral.pp3"));
        }

        // override the default with any customizations that *might* exist for the input
        opts.AddPerInputPp3Source();

        var rt = new RawTherapee(opts);

        return rt.ConvertAsync(sourceFile);
    }

    static bool IsRawFile(string file)
    {
        return file.EndsWith("nef", StringComparison.OrdinalIgnoreCase);
    }
}
