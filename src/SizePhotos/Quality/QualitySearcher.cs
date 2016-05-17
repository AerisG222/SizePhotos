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
                
                var imgmin = new Imgmin(new ImgminOptions());
                var result = imgmin.Minify(tmp, tmp);
            
                if(!_quiet)
                {
                    Console.WriteLine(result.StandardOutput);
                }
                
                return Convert.ToUInt32(result.StatsAfter.Quality);
            }
            finally
            {
                File.Delete(tmp);
            }
        }
    }
}
