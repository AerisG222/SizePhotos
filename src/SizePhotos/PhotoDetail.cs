using System;


namespace SizePhotos
{
    public class PhotoDetail
    {
        public bool IsPrivate { get; set; }
        public ExifData ExifData { get; set; }
        public PhotoInfo ThumbnailInfo { get; set; }
        public PhotoInfo FullsizeInfo { get; set; }
        public PhotoInfo FullerInfo { get; set; }
        public PhotoInfo OriginalInfo { get; set; }
        public PhotoInfo RawInfo { get; set; }
    }
}
