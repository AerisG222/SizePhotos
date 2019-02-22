using System;
using System.IO;
using System.Threading.Tasks;


namespace SizePhotos.PhotoWriters
{
    public class PhotoWriterFixedSizePhotoProcessor
        : IOutput, IPhotoProcessor
    {
        bool _quiet;
        string _scaleName;
        uint _height;
        uint _width;
        float _aspect;
        PhotoPathHelper _pathHelper;


        public string OutputSubdirectory
        {
            get
            {
                return _scaleName;
            }
        }


        public PhotoWriterFixedSizePhotoProcessor(bool quiet, string scaleName, uint height, uint width, PhotoPathHelper pathHelper)
        {
            _quiet = quiet;
            _scaleName = scaleName;
            _height = height;
            _width = width;
            _pathHelper = pathHelper;

            _aspect = _width / _height;
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
                var width = (double)tmpWand.ImageWidth;
                var height = (double)tmpWand.ImageHeight;
                var aspect = width / height;

                if(aspect >= _aspect)
                {
                    var newWidth = (width / height) * _height;

                    // scale image to final height
                    tmpWand.ScaleImage((uint) newWidth, _height);

                    // crop sides as needed
                    tmpWand.CropImage(_width, _height, (int) (newWidth - _width) / 2, 0);
                }
                else
                {
                    var newHeight = _width / (width / height);

                    // scale image to final width
                    tmpWand.ScaleImage(_width, (uint) newHeight);

                    // crop top and bottom as needed
                    tmpWand.CropImage(_width, _height, 0, (int) (newHeight - _height) / 2);
                }

                // sharpen after potentially resizing
                // http://www.imagemagick.org/Usage/resize/#resize_unsharp
                tmpWand.UnsharpMaskImage(0, 0.7, 0.7, 0.008);

                tmpWand.WriteImage(localPath, true);

                return new PhotoWriterProcessingResult(true, _scaleName, tmpWand.ImageHeight, tmpWand.ImageWidth, localPath, url);
            }
        }
    }
}