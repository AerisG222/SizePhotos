namespace SizePhotos.VisualOptimization
{
    public class OptimizationProcessingResult
        : IProcessingResult
    {
        public bool Successful { get; private set; }
        public double? SigmoidalOptimization { get; private set; }
        public double? SaturationOptimization { get; private set; }
        public string ErrorMessage { get; private set; }

        
        public bool WasOptimized
        {
            get
            {
                return SigmoidalOptimization != null || SaturationOptimization != null;
            }
        }


        public OptimizationProcessingResult(bool success, double? saturationOptimization, double? sigmoidalOptimization)
        {
            Successful = success;
            SaturationOptimization = saturationOptimization;
            SigmoidalOptimization = sigmoidalOptimization;
        }


        public OptimizationProcessingResult(string errorMessage)
        {
            Successful = false;
            ErrorMessage = errorMessage;
        }
    }
}
