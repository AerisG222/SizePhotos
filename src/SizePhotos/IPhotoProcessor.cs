using System;
using System.Threading.Tasks;


namespace SizePhotos
{
    public interface IPhotoProcessor
    {
        Task<ProcessingResult> ProcessPhotoAsync(string filename);
    }
}
