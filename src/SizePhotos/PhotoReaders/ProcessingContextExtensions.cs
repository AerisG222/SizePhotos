using System.Collections.Generic;
using System.Linq;


namespace SizePhotos.PhotoReaders;

public static class ProcessingContextExtensions
{
    public static IEnumerable<PhotoReaderProcessingResult> GetPhotoReaderResults(this ProcessingContext ctx)
    {
        return ctx.Results.OfType<PhotoReaderProcessingResult>();
    }


    public static PhotoReaderProcessingResult GetSuccessfulPhotoReaderResult(this ProcessingContext ctx)
    {
        return ctx.Results.OfType<PhotoReaderProcessingResult>().SingleOrDefault(x => x.Successful && !x.Skipped);
    }
}
