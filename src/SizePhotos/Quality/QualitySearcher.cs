using System;
using System.IO;
using NImgmin;
using NMagickWand;


// ------------------------------------------------------------------
// CREDITS:
//    - my feeble attempt sucks - all credit goes to imgmin!!
// ------------------------------------------------------------------
namespace SizePhotos.Quality
{
    public class QualitySearcher
        : IQualitySearcher
    {
        const int MAX_QUALITY = 92;
        
        
        readonly bool _quiet;
        
        
        public QualitySearcher(bool quiet)
        {
            _quiet = quiet;
        }
        
        
        public uint GetOptimalQuality(MagickWand wand)
        {
            var tmp = $"{Path.GetTempFileName()}.jpg";
            
            try
            {
                using(var tmpWand = wand.Clone())
                {
                    tmpWand.CompressionQuality = MAX_QUALITY;
                    wand.WriteImage(tmp, true);
                }
                
                var opts = new ImgminOptions
                {
                    ErrorThreshold = 0.08
                };
                
                var imgmin = new Imgmin(opts);
                var result = imgmin.Minify(tmp, tmp);
            
                if(!_quiet)
                {
                    Console.WriteLine(result.StandardOutput);
                }
                
                // the following has not been reliable, so figure out the 
                // quality based on opening the tmp file.
                //return Convert.ToUInt32(result.StatsAfter.Quality);
                
                using(var qualWand = new MagickWand(tmp))
                {
                    return qualWand.ImageCompressionQuality;
                }
            }
            finally
            {
                File.Delete(tmp);
            }
        }
    }
}
