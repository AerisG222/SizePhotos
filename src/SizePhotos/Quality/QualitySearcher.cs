using System;
using System.IO;
using NMagickWand;
using NMagickWand.Enums;


// ------------------------------------------------------------------
// CREDITS:
//    - Inspiration goes to rflynn who created imgmin
//      (a C program and Apache module) for optimizing
//      image sizes.  You can view that project and its
//      corresonding license (MIT) here:
//          https://github.com/rflynn/imgmin
//
//    - Note: I have tried to simplify the algorithm from what imgmin
//            performs.  He performs a more elaborate evaluation on 
//            differences, while I am taking a lazier approach,
//            that I hope will be simpler and reasonable for my needs.
// ------------------------------------------------------------------
namespace SizePhotos.Quality
{
    public class QualitySearcher
        : IQualitySearcher
    {
        const int DEFAULT_QUALITY = 88;
        const int MIN_UNIQUE_COLORS = 4096;
        const int STEPS = 5;
        const uint QUALITY_OUT_MAX = 92;
        const uint QUALITY_OUT_MIN = 55;
        
        // ignore (minor) differences when comparing images
        const double FUZZ = 0.05;
        
        // image should change < than 1%
        const double DISTORTION_THRESHOLD = 0.015;
        
        // distortion within 10% of threshold is acceptable
        const double DISTORTION_THRESHOLD_RANGE = DISTORTION_THRESHOLD * 0.1; 
        

        readonly bool _quiet;
        
        
        public QualitySearcher(bool quiet)
        {
            _quiet = quiet;
        }
        
        
        public uint GetOptimalQuality(MagickWand wand)
        {
            if(wand.ImageColors < MIN_UNIQUE_COLORS)
            {
                // return current quality if we don't have enough colors or pixels
                return wand.ImageCompressionQuality;
            }
            
            return FindOptimalQuality(wand);
        }
        
        
        uint FindOptimalQuality(MagickWand wand)
        {
            var numPixels = wand.ImageHeight * wand.ImageWidth;
            var maxQuality = QUALITY_OUT_MAX;
            var minQuality = QUALITY_OUT_MIN;
            int currStep = 0;
            var tmpPath = Path.GetTempFileName();
            
            if(wand.ImageCompressionQuality > QUALITY_OUT_MIN)
            {
                // in some tests we did not detect a quality for the image, (qual = 0)
                // this check makes sure the result is reasonable, and if so, set the
                // max to the source quality
                maxQuality = wand.ImageCompressionQuality;
            } 
            
            //Console.WriteLine($"starting max quality: {maxQuality}");
            
            while(maxQuality > minQuality + 1 && currStep < STEPS)
            {
                currStep++;

                uint quality = Convert.ToUInt32((maxQuality + minQuality) / 2);
                
                // write out the image at the specified quality
                using(var tmpWand = wand.Clone())
                {
                    tmpWand.ImageCompressionQuality = quality;
                    tmpWand.WriteImage(tmpPath, true);
                }
                
                double distortion;
                
                // read the test image
                using(var tmpWand = new MagickWand(tmpPath))
                {
                    tmpWand.ImageFuzz = MagickWandEnvironment.QuantumRange * FUZZ;
                    
                    var compareWand = tmpWand.CompareImages(wand, MetricType.AbsoluteErrorMetric, out distortion);
                    compareWand.Dispose();
                }
                
                File.Delete(tmpPath);
                
                var distortionPercent = distortion / numPixels;
                
                //Console.WriteLine($"quality: {quality} => distortion: {distortion} => distortion Pct: {distortionPercent}");
                
                if(distortionPercent > DISTORTION_THRESHOLD)
                {
                    minQuality = quality;
                }
                else
                {
                    maxQuality = quality;
                }
                
                // if distortion is in acceptable threshold, we've found our quality
                if(Math.Abs(distortionPercent - DISTORTION_THRESHOLD) < DISTORTION_THRESHOLD_RANGE)
                {
                    maxQuality = quality;
                    break;
                }
            }
            
            if(!_quiet)
            {
                Console.WriteLine($"quality found: {maxQuality} in {currStep} steps.");
            }
            
            return maxQuality;
        }
    }
}
