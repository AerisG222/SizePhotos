using System;
using System.IO;
using System.Threading.Tasks;
using NDCRaw;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos
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
        
        
        public async Task<string> ConvertAsync(string sourceFile)
        {
            var dcraw = new DCRaw(GetOptimalOptionsForPhoto(sourceFile));
            var ppmFile = (await dcraw.ConvertAsync(sourceFile)).OutputFilename;
            
            return ppmFile;
        }
        
        
        DCRawOptions GetOptimalOptionsForPhoto(string photoPath)
        {
            if(GetDarkThresholdForRawImage(photoPath) < DARK_THRESHOLD)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"  -using night mode for {photoPath}");    
                }
                
                return GetOptimalNightOptions();
            }
            
            return GetOptimalDayOptions();
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
        
        
        static DCRawOptions GetOptimalDayOptions()
        {
            return new DCRawOptions {
                UseCameraWhiteBalance = true,
                Quality = InterpolationQuality.Quality3,
                HighlightMode = HighlightMode.Blend,
                Colorspace = Colorspace.sRGB
            };
        }
        
        
        static DCRawOptions GetOptimalNightOptions()
        {
            var opts = GetOptimalDayOptions();
            
            opts.DontAutomaticallyBrighten = true;
            
            return opts;
        }
    }
}
