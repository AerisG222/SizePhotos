using System.Threading.Tasks;


namespace SizePhotos.Minification
{
    public class StripMetadataPhotoProcessor
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
                context.Wand.StripImage();

                return Task.FromResult((IProcessingResult) new StripMetadataProcessingResult(true));
            }

            return Task.FromResult((IProcessingResult) new StripMetadataProcessingResult(false));
        }
    }
}
