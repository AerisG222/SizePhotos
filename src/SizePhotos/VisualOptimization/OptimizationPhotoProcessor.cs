using System;
using System.Threading.Tasks;
using NMagickWand;
using NMagickWand.Enums;


namespace SizePhotos.VisualOptimization
{
    public class OptimizationPhotoProcessor
        : IPhotoProcessor
    {
        // this seems to work well for both brightness and saturation
        const double THRESHOLD_DECIMAL = 0.30;
        const double MIN_MEAN_FOR_BRIGHTENING_ADJUSTMENT = 2000;
        const double MAX_SIGMOIDAL_ADJUSTMENT = 3;
        const double MAX_SATURATION_ADJUSTMENT = 20;
        
        bool _quiet;
        
        
        public OptimizationPhotoProcessor(bool quiet)
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
                return Task.FromResult((IProcessingResult) Optimize(ctx.Wand));
            }
            catch(Exception ex)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"Error optimizing photo for file {ctx.SourceFile}.  Error Message: {ex.Message}");
                }

                return Task.FromResult((IProcessingResult) new OptimizationProcessingResult(false, null, null));
            }
        }

        
        OptimizationProcessingResult Optimize(MagickWand wand)
        {
            wand.AutoLevelImage();
            
            var sat = AdjustSaturation(wand);
            var sig = AdjustBrightness(wand);
            
            return new OptimizationProcessingResult(true, sat, sig);
        }
        
        
        double? AdjustSaturation(MagickWand wand)
        {
            double mean, stddev;

            wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
            
            var saturationAdjustment = GetSaturationAdjustment(stddev, stddev / MagickWandEnvironment.QuantumRange);
            
            if(saturationAdjustment > 100d)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"adjusting saturation: {saturationAdjustment}");
                }
                
                // 300 = don't rotate hue
                wand.ModulateImage(100, saturationAdjustment, 300);
                
                return saturationAdjustment;
            }

            return null;
        }
        
        
        double? AdjustBrightness(MagickWand wand)
        {
            double mean, stddev;

            wand.GetImageChannelMean(ChannelType.AllChannels, out mean, out stddev);
            
            var sigmoidalBrightnessAdjustment = GetSigmoidalAdjustment(mean, mean / MagickWandEnvironment.QuantumRange);
            
            // adjust brightness using sigmoidal contrast so we don't blow highlights, like we sometimes
            // would do when we tried to adjust using the brightness parameter of ModulateImage
            if(sigmoidalBrightnessAdjustment > 1d)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"adjusting sigmoidal: {sigmoidalBrightnessAdjustment}");
                }
                
                wand.SigmoidalContrastImage(ChannelType.AllChannels, true, sigmoidalBrightnessAdjustment, 0);
                
                return sigmoidalBrightnessAdjustment;
            }

            return null;
        }
        
        
        double GetSigmoidalAdjustment(double mean, double meanQuantumPercent)
        {
            double adjustment = 0;
            
            // adjust pct to favor brightening
            meanQuantumPercent -= .15;
            meanQuantumPercent = Math.Max(0, meanQuantumPercent);
            
            if(meanQuantumPercent < THRESHOLD_DECIMAL && mean > MIN_MEAN_FOR_BRIGHTENING_ADJUSTMENT)
            {
                adjustment = CalculateAdjustment(meanQuantumPercent, MAX_SIGMOIDAL_ADJUSTMENT);
            }
            
            return adjustment;
        }
        
        
        double GetSaturationAdjustment(double stddev, double stddevQuantumPercent)
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
