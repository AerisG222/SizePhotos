using System;
using System.Threading.Tasks;


namespace SizePhotos.Exif
{
    public interface IExifReader
    {
        Task<ExifData> ReadExifDataAsync(string sourcePhoto);
    }
}