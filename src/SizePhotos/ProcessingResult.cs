using System;
using System.Collections.Generic;


namespace SizePhotos
{
    public class ProcessingResult
    {
        public bool IsPrivate { get; set; }
        public ExifData ExifData { get; set; }
        public ProcessedPhoto Source { get; set; }
        public ProcessedPhoto Xs { get; set; }
        public ProcessedPhoto Sm { get; set; }
        public ProcessedPhoto Md { get; set; }
        public ProcessedPhoto Lg { get; set; }
    }
}
