using System.Linq;


namespace SizePhotos.VisualOptimization
{
    public static class ProcessingContextExtensions
    {
        public static OptimizationProcessingResult GetOptimizationResult(this ProcessingContext ctx)
        {
            return ctx.Results.OfType<OptimizationProcessingResult>().FirstOrDefault();
        }
    }
}
