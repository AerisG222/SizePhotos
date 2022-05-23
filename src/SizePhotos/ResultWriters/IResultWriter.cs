namespace SizePhotos.ResultWriters;

public interface IResultWriter
{
    void PreProcess(CategoryInfo category);
    void AddResult(ProcessingContext ctx);
    void PostProcess();
}
