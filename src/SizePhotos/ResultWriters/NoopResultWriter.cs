using System.Collections.Generic;

namespace SizePhotos.ResultWriters;

public class NoopResultWriter
    : IResultWriter
{
    public void WriteOutput(string file, CategoryInfo category, IEnumerable<ProcessedPhoto> photos)
    {
        // do nothing
    }
}
