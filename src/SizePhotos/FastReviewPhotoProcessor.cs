using System;
using System.IO;
using System.Threading.Tasks;
using NMagickWand;
using SizePhotos.Raw;


namespace SizePhotos
{
    public class FastReviewPhotoProcessor
        : IPhotoProcessor
    {
        readonly PhotoPathHelper _pathHelper;
        readonly IRawConverter _rawConverter;


        public FastReviewPhotoProcessor(PhotoPathHelper pathHelper,
            IRawConverter rawConverter)
        {
            _pathHelper = pathHelper;
            _rawConverter = rawConverter;
        }


        public async Task<ProcessingResult> ProcessPhotoAsync(string filename)
        {
            using(var wand = new MagickWand())
            {
                var srcFile = _pathHelper.GetSourceFilePath(filename);

                if(_rawConverter.IsRawFile(srcFile))
                {
                    var conversionResult = await _rawConverter.ConvertAsync(srcFile);
                    
                    wand.ReadImage(conversionResult.OutputFile);
                    File.Delete(conversionResult.OutputFile);
                } 
                else 
                {
                    wand.ReadImage(srcFile);
                }
                
                wand.AutoOrientImage();
                wand.AutoLevelImage();
                wand.StripImage();
                
                var path = Path.Combine(Path.GetDirectoryName(srcFile), "review", $"{Path.GetFileNameWithoutExtension(filename)}.jpg");

                wand.WriteImage(path, true);
            }
            
            return null;
        }
    }
}
