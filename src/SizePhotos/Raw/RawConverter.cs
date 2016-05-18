using System;
using System.IO;
using System.Threading.Tasks;
using NDCRaw;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos.Raw
{
    public class RawConverter
        : IRawConverter
    {
        readonly bool _quiet;
        
        
        public RawConverter(bool quiet)
        {
            _quiet = quiet;
        }


        public bool IsRawFile(string file)
        {
            return file.EndsWith("nef", StringComparison.InvariantCultureIgnoreCase);
        }
        
        
        public async Task<IRawConversionResult> ConvertAsync(string sourceFile)
        {
            var result = new RawConversionResult();
            var opts = new DCRawOptions {
                UseCameraWhiteBalance = true,
                Quality = InterpolationQuality.Quality3,
                HighlightMode = HighlightMode.Blend,
                Colorspace = Colorspace.sRGB,
                DontAutomaticallyBrighten = true
            };
            var dcraw = new DCRaw(opts);
            
            result.OutputFile = (await dcraw.ConvertAsync(sourceFile)).OutputFilename;
            
            return result;
        }
    }
}
