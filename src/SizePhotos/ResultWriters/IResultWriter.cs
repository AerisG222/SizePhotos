using System.Collections.Generic;

namespace SizePhotos.ResultWriters;

public interface IResultWriter
{
    void WriteOutput(string outputFile, CategoryInfo category, IEnumerable<ProcessedPhoto> photos);
}
