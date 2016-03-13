using System;
using System.IO;


namespace SizePhotos
{
    public class ProcessingTarget
    {
        public uint? Quality { get; set; }
        public bool Optimize { get; set; }
        public string LocalPath { get; set; }
        public string WebPath { get; set; }
        public uint MaxHeight { get; set; }
        public uint MaxWidth { get; set; }
        
        
        public string GetLocalPathForPhoto(string img)
        {
            return Path.Combine(LocalPath, Path.GetFileName(img));
        }
        
        
        public string GetWebPathForPhoto(string img)
        {
            return $"{WebPath}{Path.GetFileName(img)}";
        }
    }
}