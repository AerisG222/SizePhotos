using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SizePhotos;

class PhotoProcessingPipeline
{
    readonly List<IPhotoProcessor> _processors = new List<IPhotoProcessor>();


    public void AddProcessor(IPhotoProcessor processor)
    {
        _processors.Add(processor);
    }


    public async Task<ProcessingContext> ProcessPhotoAsync(string photoPath)
    {
        var ctx = new ProcessingContext(photoPath);

        foreach (var processor in _processors)
        {
            var proc = processor.Clone();

            var result = await proc.ProcessPhotoAsync(ctx);

            if (result != null)
            {
                ctx.AddResult(result);

                if (!result.Successful)
                {
                    break;
                }
            }
        }

        return ctx;
    }


    internal IEnumerable<IOutput> GetOutputProcessors()
    {
        return _processors.OfType<IOutput>();
    }
}
