using System;
using System.Threading.Tasks;


namespace SizePhotos
{
    public interface IExifReader
    {
        Task<ExifData> ReadExifDataAsync(string sourcePhoto);
    }
}