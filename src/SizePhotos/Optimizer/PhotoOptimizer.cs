using System;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos.Optimizer
{
    public class PhotoOptimizer
        : IPhotoOptimizer
    {
        // this seems to work well for both brightness and saturation
        const double THRESHOLD_DECIMAL = 0.30;
        const double MIN_MEAN_FOR_BRIGHTENING_ADJUSTMENT = 2000;
        const double MAX_SIGMOIDAL_ADJUSTMENT = 3;
        const double MAX_SATURATION_ADJUSTMENT = 20;
        
        readonly bool _quiet;
        
        
        public PhotoOptimizer(bool quiet)
        {
            _quiet = quiet;
        }
        
        
        public IOptimizationResult Optimize(MagickWand wand)
        {
            double mean, stddev;
            double quantumRange = MagickWandEnvironment.QuantumRange;
            var result = new OptimizationResult();
            
            wand.AutoLevelImage();
            wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
            
            var sigmoidalBrightnessAdjustment = GetSigmoidalAdjustment(mean, mean / quantumRange);
            var saturationAdjustment = GetSaturationAdjustment(stddev, stddev / quantumRange);
            
            // adjust brightness using sigmoidal contrast so we don't blow highlights, like we sometimes
            // would do when we tried to adjust using the brightness parameter of ModulateImage
            if(sigmoidalBrightnessAdjustment > 1d)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"adjusting sigmoidal: {sigmoidalBrightnessAdjustment}");
                }
                
                wand.SigmoidalContrastImage(true, sigmoidalBrightnessAdjustment, 0);
                
                result.SigmoidalOptimization = sigmoidalBrightnessAdjustment;
            }
            
            if(saturationAdjustment > 100d)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"adjusting saturation: {saturationAdjustment}");
                }
                
                // 300 = don't rotate hue
                wand.ModulateImage(100, saturationAdjustment, 300);
                
                result.SaturationOptimization = saturationAdjustment;
            }
            
            return result;
        }
        
        
        public double GetSigmoidalAdjustment(double mean, double meanQuantumPercent)
        {
            double adjustment = 0;
            
            if(meanQuantumPercent < THRESHOLD_DECIMAL && mean > MIN_MEAN_FOR_BRIGHTENING_ADJUSTMENT)
            {
                adjustment = CalculateAdjustment(meanQuantumPercent, MAX_SIGMOIDAL_ADJUSTMENT);
            }
            
            return adjustment;
        }
        
        
        public double GetSaturationAdjustment(double stddev, double stddevQuantumPercent)
        {
            double adjustment = 0;
            
            if(stddevQuantumPercent < THRESHOLD_DECIMAL)
            {
                adjustment = CalculateAdjustment(stddevQuantumPercent, MAX_SATURATION_ADJUSTMENT);
            }
            
            // 100% => no change in saturation
            return 100.0 + adjustment;
        }
        
        
        double CalculateAdjustment(double pct, double maxAdjustment)
        {
            // see opt.gnu / opt.png in the tools directory illustrates the adjustment curve
            var pctAdjustment = 0.5 + Math.Cos(pct / (THRESHOLD_DECIMAL/Math.PI)) / 2d;
            
            return maxAdjustment * pctAdjustment;
        }
    }
}
