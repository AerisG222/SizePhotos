using System.Linq;

namespace SizePhotos.Exif;

public static class ProcessingContextExtensions
{
    public static ExifProcessingResult GetExifResult(this ProcessingContext ctx)
    {
        return ctx.Results.OfType<ExifProcessingResult>().FirstOrDefault();
    }
}
