using System;
using System.IO;
using System.Threading.Tasks;
using NImgmin;
using NMagickWand;


namespace SizePhotos.Minification
{
    public class JpgQualityPhotoProcessor
        : IPhotoProcessor
    {
        const int MAX_QUALITY = 92;
        
        
        bool _quiet;
        
        
        public JpgQualityPhotoProcessor(bool quiet)
        {
            _quiet = quiet;
        }
        

        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor) MemberwiseClone();
        }

        
        public Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext ctx)
        {
            try
            {
                var result = GetOptimalQuality(ctx.Wand);

                return Task.FromResult((IProcessingResult) new JpgQualityProcessingResult(true, result));
            }
            catch(Exception ex)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"Error finding min jpg quality setting for file {ctx.SourceFile}.  Error Message: {ex.Message}");
                }

                return Task.FromResult((IProcessingResult) new JpgQualityProcessingResult(false, 0));
            }
        }


        uint GetOptimalQuality(MagickWand wand)
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
