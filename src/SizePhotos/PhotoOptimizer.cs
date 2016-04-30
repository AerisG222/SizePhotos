using System;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos
{
    public class PhotoOptimizer
        : IPhotoOptimizer
    {
        public void Optimize(MagickWand wand)
        {
            // the following thresholds are not scientific (big surprise).  They were derrived by running 
            // a dozen sample files and reviewing mean+stddev values and the images to determine a reasonable
            // way to adjust the image while trying not to overprocess the images
            var brightnessToContrastThreshold = 0.5;
            var contrastStrengthThreshold = 1.9;
            double mean, stddev;
            
            wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
            wand.AutoLevelImage();
            
            var brightnessToContrast = stddev / mean;
            var contrastStrength = stddev / 10000;
            
            if(brightnessToContrast < brightnessToContrastThreshold)
            {
                var saturationAmount = Convert.ToInt32((brightnessToContrastThreshold - brightnessToContrast) * 100) * 4;
                
                // limit the saturation adjustment to 20%
                if(saturationAmount > 20)
                {
                    saturationAmount = 20;
                }
                
                saturationAmount += 100;
                
                // 100 = don't adjust brightness
                // 300 = don't rotate hue
                wand.ModulateImage(100, saturationAmount, 300);
            }
            else if(contrastStrength > contrastStrengthThreshold)
            {
                wand.SigmoidalContrastImage(true, 2, 0);  // smooth brightness/contrast
            }
        }
    }
}
