using System;
using System.IO;
using System.Threading.Tasks;
using NMagickWand;
using SizePhotos.Minification;


namespace SizePhotos.PhotoWriters
{
    public class PhotoWriterPhotoProcessor
        : IOutput, IPhotoProcessor
    {
        bool _quiet;
        string _scaleName;
        PhotoPathHelper _pathHelper;


        public string OutputSubdirectory
        {
            get
            {
                return _scaleName;
            }
        }


        public PhotoWriterPhotoProcessor(bool quiet, string scaleName, PhotoPathHelper pathHelper)
        {
            _quiet = quiet;
            _scaleName = scaleName;
            _pathHelper = pathHelper;
        }


        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor) MemberwiseClone();
        }


        public Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext ctx)
        {
            try
            {
                return Task.FromResult((IProcessingResult) ScalePhoto(ctx));
            }
            catch(Exception ex)
            {
                return Task.FromResult((IProcessingResult) new PhotoWriterProcessingResult($"Error writing file for scale {_scaleName}: {ex.Message}"));
            }
        }


        PhotoWriterProcessingResult ScalePhoto(ProcessingContext ctx)
        {
            var filename = Path.GetFileName(ctx.SourceFile);
            var jpgName = Path.ChangeExtension(filename, ".jpg");
            var localPath = _pathHelper.GetScaledLocalPath(_scaleName, jpgName);
            var url = _pathHelper.GetScaledWebFilePath(_scaleName, jpgName);

            using(var tmpWand = ctx.Wand.Clone())
            {
                // sharpen after potentially resizing
                // http://www.imagemagick.org/Usage/resize/#resize_unsharp
                tmpWand.UnsharpMaskImage(0, 0.7, 0.7, 0.008);

                tmpWand.WriteImage(localPath, true);

                var file = new FileInfo(localPath);

                return new PhotoWriterProcessingResult(true, _scaleName, tmpWand.ImageHeight, tmpWand.ImageWidth, file.Length, localPath, url);
            }
        }
    }
}
