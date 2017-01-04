using System.Linq;


namespace SizePhotos.Minification
{
    public static class ProcessingContextExtensions
    {
        public static JpgQualityProcessingResult GetJpgQualityResult(this ProcessingContext ctx)
        {
            return ctx.Results.OfType<JpgQualityProcessingResult>().FirstOrDefault();
        }
    }
}
