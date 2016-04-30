using System;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos
{
    public class PhotoOptimizer
        : IPhotoOptimizer
    {
        // the following thresholds are not scientific (big surprise).  They were derrived by running 
        // a dozen sample files and reviewing mean+stddev values and the images to determine a reasonable
        // way to adjust the image while trying not to overprocess the images
        const double CONTRAST_TO_BRIGHTNESS_THRESHOLD = 0.5;
        const double CONTRAST_STRENGTH_THRESHOLD = 1.9;
        
        
        public void Optimize(MagickWand wand)
        {
            double mean, stddev;
            
            wand.AutoLevelImage();
            wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
            
            var contrastRatio = stddev / 10000;
            
            // if the image is too contrasty, try to smooth this out a little
            if(contrastRatio > CONTRAST_STRENGTH_THRESHOLD)
            {
                wand.SigmoidalContrastImage(true, 2, 0);
                return;
            }
            
            var brightnessAdjustment = GetBrightnessAdjustment(mean, stddev);
            var saturationAdjustment = GetSaturationAdjustment(mean, stddev);
            
            if(brightnessAdjustment != 100 || saturationAdjustment != 100)
            {
                Console.WriteLine($"modulating: brightness={brightnessAdjustment}, saturation={saturationAdjustment}");
                
                // 300 = don't rotate hue
                wand.ModulateImage(brightnessAdjustment, saturationAdjustment, 300);
            }
        }
        
        
        public short GetBrightnessAdjustment(double mean, double stddev)
        {
            short adjustment = 100;  // 100 = 100% => no change in saturation
            short brighten = 0;
            
            if(mean > 2000 && mean < 20000)
            {
                // we want at most the image to be brightened by 50%
                // 18,000 / 360 = 50
                brighten = (short) ((20000 - mean - 2000) / 360);  
            }
            
            return (short) (adjustment + brighten);
        }
        
        
        public short GetSaturationAdjustment(double mean, double stddev)
        {
            short adjustment = 100;  // 100 = 100% => no change in saturation
            var contrastToBrightness = stddev / mean;
            
            if(contrastToBrightness < CONTRAST_TO_BRIGHTNESS_THRESHOLD)
            {
                short saturationAmount = (short) ((CONTRAST_TO_BRIGHTNESS_THRESHOLD - contrastToBrightness) * 100 * 4);
                
                // limit the saturation adjustment to 20%
                if(saturationAmount > 24)
                {
                    saturationAmount = 24;
                }
                
                adjustment += saturationAmount;
            }
            
            return adjustment;
        }
    }
}
