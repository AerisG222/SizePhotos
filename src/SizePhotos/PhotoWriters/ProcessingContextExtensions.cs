using System;
using System.Collections.Generic;
using System.Linq;


namespace SizePhotos.PhotoWriters
{
    public static class ProcessingContextExtensions
    {
        public static IEnumerable<PhotoWriterProcessingResult> GetPhotoWriterResults(this ProcessingContext ctx)
        {
            return ctx.Results.OfType<PhotoWriterProcessingResult>();
        }


        public static PhotoWriterProcessingResult GetPhotoWriterResult(this ProcessingContext ctx, string scaleName)
        {
            return ctx.GetPhotoWriterResults().SingleOrDefault(x => string.Equals(x.ScaleName, scaleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
