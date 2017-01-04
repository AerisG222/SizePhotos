namespace SizePhotos
{
    public class ContextTerminatorProcessingResult
        : IProcessingResult
    {
        public bool Successful { get; private set; }


        public ContextTerminatorProcessingResult(bool success)
        {
            Successful = success;
        }
    }
}
