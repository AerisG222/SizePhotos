using System;
using System.IO;
using System.Threading.Tasks;
using NMagickWand;
using NRawTherapee;
using NRawTherapee.OutputFormat;


namespace SizePhotos.PhotoReaders
{
    public class RawTherapeePhotoReaderPhotoProcessor
        : IPhotoProcessor
    {
        bool _quiet;
        PhotoPathHelper _pathHelper;


        public RawTherapeePhotoReaderPhotoProcessor(bool quiet, PhotoPathHelper pathHelper)
        {
            _quiet = quiet;
            _pathHelper = pathHelper;
        }


        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor)MemberwiseClone();
        }


        public async Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext ctx)
        {
            string output = null;

            try
            {
                output = await ReadAsync(ctx.SourceFile);

                var wand = new MagickWand();
                wand.ReadImage(output);
                wand.AutoOrientImage();

                ctx.Wand = wand;

                // TODO: fix 'src' hardcoding
                var url = _pathHelper.GetScaledWebFilePath("src", Path.GetFileName(ctx.SourceFile));

                return new PhotoReaderProcessingResult(true, false, wand.ImageHeight, wand.ImageWidth, url);
            }
            catch (Exception ex)
            {
                return new PhotoReaderProcessingResult($"Error reading file: {ex.Message}");
            }
            finally
            {
                if (output != null && File.Exists(output))
                {
                    File.Delete(output);
                }
            }
        }


        async Task<string> ReadAsync(string sourceFile)
        {
            var opts = new Options();
            var filename = $"{Path.GetFileNameWithoutExtension(sourceFile)}_{Guid.NewGuid().ToString("N")}.tif";

            opts.RawTherapeePath = "rawtherapee-cli";
            opts.OutputFormat = new TiffOutputFormat();
            opts.OutputFile = Path.Combine(Path.GetDirectoryName(sourceFile), filename);

            if(RawHelper.IsRawFile(sourceFile))
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
            var result = await rt.ConvertAsync(sourceFile);

            return result.OutputFilename;
        }
    }
}
