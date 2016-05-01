using System;
using SizePhotos.Exif;
using SizePhotos.Optimizer;
using SizePhotos.Raw;


namespace SizePhotos
{
    public class ProcessingResult
    {
        public ExifData ExifData { get; set; }
        public IRawConversionResult RawConversionResult { get; set; }
        public IOptimizationResult OptimizationResult { get; set; }
        public ProcessedPhoto Source { get; set; }
        public ProcessedPhoto Xs { get; set; }
        public ProcessedPhoto Sm { get; set; }
        public ProcessedPhoto Md { get; set; }
        public ProcessedPhoto Lg { get; set; }
    }
}
