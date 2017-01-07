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
        uint _maxHeight;
        uint _maxWidth;
        bool _adjustQuality;
        PhotoPathHelper _pathHelper;


        public string OutputSubdirectory 
        { 
            get
            {
                return _scaleName;
            }
        }
        

        public PhotoWriterPhotoProcessor(bool quiet, string scaleName, uint maxHeight, uint maxWidth, bool adjustQuality, PhotoPathHelper pathHelper)
        {
            _quiet = quiet;
            _scaleName = scaleName;
            _maxHeight = maxHeight;
            _maxWidth = maxWidth;
            _adjustQuality = adjustQuality;
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
                uint width, height;
                
                if(_maxWidth > 0)
                {
                    tmpWand.GetLargestDimensionsKeepingAspectRatio(_maxWidth, _maxHeight, out width, out height);
                    tmpWand.ScaleImage(width, height);
                }
                else
                {
                    width = ctx.Wand.ImageWidth;
                    height = ctx.Wand.ImageHeight;
                }
                
                // sharpen after potentially resizing
                // http://www.imagemagick.org/Usage/resize/#resize_unsharp
                tmpWand.UnsharpMaskImage(0, 0.7, 0.7, 0.008);
                
                AdjustQuality(ctx, tmpWand);
                
                tmpWand.WriteImage(localPath, true);
                
                return new PhotoWriterProcessingResult(true, _scaleName, tmpWand.ImageHeight, tmpWand.ImageWidth, localPath, url);
            }
        }


        void AdjustQuality(ProcessingContext ctx, MagickWand wandToAdjust)
        {
            if(!_adjustQuality)
            {
                return;
            }

            var result = ctx.GetJpgQualityResult();

            if(result != null && result.Successful)
            {
                wandToAdjust.ImageCompressionQuality = Convert.ToUInt32(result.MinQualitySetting);
            }
        }
    }
}
