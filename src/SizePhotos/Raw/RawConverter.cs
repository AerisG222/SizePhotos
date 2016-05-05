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
        const double DARK_THRESHOLD = 250;
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
            
            result.Mode = DetermineConversionMode(sourceFile);
            
            if(!_quiet && result.Mode == RawConversionMode.NonBrightening)
            {
                Console.WriteLine($"{sourceFile}: raw conversion using non-brightening mode");
            }
            
            var dcraw = new DCRaw(GetOptimalOptions(result.Mode));
            
            result.OutputFile = (await dcraw.ConvertAsync(sourceFile)).OutputFilename;
            
            return result;
        }
        
        
        RawConversionMode DetermineConversionMode(string photoPath)
        {
            if(GetDarkThresholdForRawImage(photoPath) < DARK_THRESHOLD)
            {
                return RawConversionMode.NonBrightening;
            }
            
            return RawConversionMode.Brightening;
        }
        
        
        static double GetDarkThresholdForRawImage(string path)
        {
            var opts = new DCRawOptions {
                HalfSizeColorImage = true,  // try to speed this up, don't need quality here
                UseCameraWhiteBalance = true,
                DontAutomaticallyBrighten = true
            };
            
            var dcraw = new DCRaw(opts);
            var res = dcraw.Convert(path);
            
            using(var wand = new MagickWand(res.OutputFilename))
            {
                double mean, stddev;
                
                // read the image, posterize, then figure out how 'dark' it is
                wand.PosterizeImage(3, false);
                wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
                
                File.Delete(res.OutputFilename);

                return mean;
            }
        }
        
        
        static DCRawOptions GetOptimalOptions(RawConversionMode mode)
        {
            if(mode == RawConversionMode.NonBrightening)
            {
                return GetOptimalNonBrighteningOptions();
            }
            
            return GetOptimalBrighteningOptions();
        }
        
        
        static DCRawOptions GetOptimalBrighteningOptions()
        {
            return new DCRawOptions {
                UseCameraWhiteBalance = true,
                Quality = InterpolationQuality.Quality3,
                HighlightMode = HighlightMode.Blend,
                Colorspace = Colorspace.sRGB
            };
        }
        
        
        static DCRawOptions GetOptimalNonBrighteningOptions()
        {
            var opts = GetOptimalBrighteningOptions();
            
            opts.DontAutomaticallyBrighten = true;
            
            return opts;
        }
    }
}
