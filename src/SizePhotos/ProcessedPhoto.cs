using SizePhotos.Exif;
using SizePhotos.PhotoWriters;

namespace SizePhotos;

public class ProcessedPhoto
{
    public ExifData ExifData { get; set; }
    public ResizeResult XsSq { get; set; }
    public ResizeResult Xs { get; set; }
    public ResizeResult Sm { get; set; }
    public ResizeResult Md { get; set; }
    public ResizeResult Lg { get; set; }
    public ResizeResult Prt { get; set; }
    public ResizeResult Src { get; set; }
}
