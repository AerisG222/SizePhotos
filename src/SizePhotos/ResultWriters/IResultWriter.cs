using System;


namespace SizePhotos.ResultWriters
{
    public interface IResultWriter
    {
        void PreProcess(CategoryInfo category);
        void AddResult(ProcessingResult result);
        void PostProcess();
    }
}
