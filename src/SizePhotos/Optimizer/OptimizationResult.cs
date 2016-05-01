using System;


namespace SizePhotos.Optimizer
{
    public class OptimizationResult
        : IOptimizationResult
    {
        public double? SigmoidalOptimization { get; set; }
        public double? SaturationOptimization { get; set; }
        
        
        public bool WasOptimized
        {
            get
            {
                return SigmoidalOptimization != null || SaturationOptimization != null;
            }
        }
    }
}
