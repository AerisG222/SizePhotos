using System.Threading.Tasks;


namespace SizePhotos
{
    public class ContextTerminatorPhotoProcessor
        : IPhotoProcessor
    {
        public IPhotoProcessor Clone()
        {
            return (IPhotoProcessor) MemberwiseClone();
        }


        public Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext context)
        {
            if(context.Wand != null)
            {
                context.Wand.Dispose();
                context.Wand = null;
            }

            return Task.FromResult((IProcessingResult) new ContextTerminatorProcessingResult(true));
        }
    }
}
