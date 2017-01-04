using System;
using System.IO;
using System.Threading.Tasks;
using NMagickWand;


namespace SizePhotos.PhotoReaders
{
    public class PhotoReaderPhotoProcessor
        : IPhotoProcessor
    {
        bool _quiet;
        PhotoPathHelper _pathHelper;


        public PhotoReaderPhotoProcessor(bool quiet, PhotoPathHelper pathHelper)
        {
            _quiet = quiet;
            _pathHelper = pathHelper;
        }


        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor)MemberwiseClone();
        }


        public Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext context)
        {
            try
            {
                return Task.FromResult(ReadPhoto(context));
            }
            catch (Exception ex)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"Error trying to read file {context.SourceFile}.  Error: {ex.Message}");
                }

                return Task.FromResult((IProcessingResult) new PhotoReaderProcessingResult(false, false));
            }
        }


        IProcessingResult ReadPhoto(ProcessingContext ctx)
        {
            if (ctx.Wand != null)
            {
                return new PhotoReaderProcessingResult(true, true);
            }

            var wand = new MagickWand();

            wand.ReadImage(ctx.SourceFile);
            wand.AutoOrientImage();

            ctx.Wand = wand;

            var url = _pathHelper.GetScaledWebFilePath("src", Path.GetFileName(ctx.SourceFile));

            return new PhotoReaderProcessingResult(true, false, wand.ImageHeight, wand.ImageWidth, url);
        }
    }
}
