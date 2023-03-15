using System.Threading.Tasks;

namespace SizePhotos.Processors;

public interface IPhotoProcessor
{
    void PrepareDirectories(string sourceDirectory);
    Task<ProcessedPhoto> ProcessAsync(string sourceFile);
}
