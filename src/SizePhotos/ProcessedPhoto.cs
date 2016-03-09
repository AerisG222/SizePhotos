using System;


namespace SizePhotos
{
    public class ProcessedPhoto
    {
        public ProcessingTarget Target { get; set; }
        public uint Height { get; set; }
        public uint Width { get; set; }
        public string Filename { get; set; }
        
        
        public string LocalPath
        {
            get
            {
                return Target.GetLocalPathForPhoto(Filename);
            }
        }
        
        
        public string WebPath
        {
            get
            {
                return Target.GetWebPathForPhoto(Filename);
            }
        }
    }
}
