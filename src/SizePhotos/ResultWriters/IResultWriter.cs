using System;


namespace SizePhotos.ResultWriters
{
    public interface IResultWriter
    {
        void PreProcess(CategoryInfo category);
        void Write(ProcessingResult result);
        void PostProcess();
    }
}
