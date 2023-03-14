using System;
using System.IO;
using System.Threading.Tasks;
using NRawTherapee;
using NRawTherapee.OutputFormat;

namespace SizePhotos.PhotoReaders;

public class RawTherapeeConverter
{
    public Task ConvertAsync(string sourceFile, string destFile)
    {
        var opts = new Options
        {
            RawTherapeePath = "rawtherapee-cli",
            OutputFormat = new TiffOutputFormat(),
            OutputFile = destFile
        };

        if (IsRawFile(sourceFile))
        {
            // default to a pre-specified profile (copy of "/usr/share/rawtherapee/profiles/Generic/Natural 1.pp3")
            // opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, "natural.pp3"));
            opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, "auto_matched_curve_iso_low.pp3"));

            // now bump the contrast and saturation a bit
            opts.AddUserSpecifiedPp3Source(Path.Combine(AppContext.BaseDirectory, "contrast_saturation.pp3"));
        }
        else
        {
            // default to a neutral profile (generated in app)
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
