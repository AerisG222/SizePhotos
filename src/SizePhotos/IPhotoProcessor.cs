using System.Threading.Tasks;


namespace SizePhotos;

public interface IPhotoProcessor
{
    Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext context);
    IPhotoProcessor Clone();
}
