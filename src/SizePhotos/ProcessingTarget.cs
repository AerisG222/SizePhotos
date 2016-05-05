using System;
using System.IO;


namespace SizePhotos
{
    public class ProcessingTarget
    {
        public bool Optimize { get; set; }
        public string ScaledPathSegment { get; set; }
        public uint MaxHeight { get; set; }
        public uint MaxWidth { get; set; }
    }
}
