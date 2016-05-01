using System;


namespace SizePhotos.Raw
{
    public class RawConversionResult
        : IRawConversionResult
    {
        public string OutputFile { get; set; }
        public RawConversionMode Mode { get; set; }
    }
}
