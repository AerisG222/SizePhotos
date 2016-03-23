using System;


namespace SizePhotos
{
    public class ProcessedPhoto
    {
        public ProcessingTarget Target { get; set; }
        public uint Height { get; set; }
        public uint Width { get; set; }
        public string LocalFilePath { get; set; }
        public string WebFilePath { get; set; }
    }
}
