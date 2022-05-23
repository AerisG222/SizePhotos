using System.Collections.Generic;
using System.IO;

namespace SizePhotos.ResultWriters;

public abstract class BaseResultWriter
    : IResultWriter
{
    protected List<ProcessingContext> _results = new List<ProcessingContext>();
    protected StreamWriter _writer;

    public abstract void AddResult(ProcessingContext ctx);
    public abstract void PostProcess();
    public abstract void PreProcess(CategoryInfo category);
}
