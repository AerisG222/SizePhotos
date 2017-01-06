using System;
using System.IO;
using System.Threading.Tasks;
using NDCRaw;
using NMagickWand;


namespace SizePhotos.PhotoReaders
{
    public class DcrawPhotoReaderPhotoProcessor
        : IPhotoProcessor
    {
        bool _quiet;
        bool _isReviewMode;
        PhotoPathHelper _pathHelper;
        
        
        public DcrawPhotoReaderPhotoProcessor(bool quiet, bool isReviewMode, PhotoPathHelper pathHelper)
        {
            _quiet = quiet;
            _isReviewMode = isReviewMode;
            _pathHelper = pathHelper;
        }


        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor) MemberwiseClone();
        }

        
        public async Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext ctx)
        {
            string output = null;

            if(RawHelper.IsRawFile(ctx.SourceFile))
            {
                try
                {
                    output = await ConvertRawAsync(ctx.SourceFile);

                    var wand = new MagickWand();
                    wand.ReadImage(output);
                    wand.AutoOrientImage();

                    if(_isReviewMode)
                    {
                        wand.AutoLevelImage();
                    }
                    
                    ctx.Wand = wand;

                    // TODO: fix 'src' hardcoding
                    var url = _pathHelper.GetScaledWebFilePath("src", Path.GetFileName(ctx.SourceFile));

                    return new PhotoReaderProcessingResult(true, false, wand.ImageHeight, wand.ImageWidth, url);
                }
                catch(Exception ex)
                {
                    if(!_quiet)
                    {
                        Console.WriteLine($"Error converting from raw for file {ctx.SourceFile}.  Error Message: {ex.Message}");
                    }

                    return new PhotoReaderProcessingResult(false, false);
                }
                finally
                {
                    if(output != null && File.Exists(output))
                    {
                        File.Delete(output);
                    }
                }
            }
            else {
                return new PhotoReaderProcessingResult(true, true);
            }
        }


        async Task<string> ConvertRawAsync(string sourceFile)
        {
            var opts = new DCRawOptions {
                UseCameraWhiteBalance = true,
                Quality = InterpolationQuality.Quality3,
                HighlightMode = HighlightMode.Blend,
                Colorspace = Colorspace.sRGB,
                DontAutomaticallyBrighten = true,
                HalfSizeColorImage = _isReviewMode
            };
            
            var dcraw = new DCRaw(opts);
            var result = await dcraw.ConvertAsync(sourceFile);

            return result.OutputFilename;
        }
    }
}
