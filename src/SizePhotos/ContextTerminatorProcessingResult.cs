namespace SizePhotos
{
    public class ContextTerminatorProcessingResult
        : IProcessingResult
    {
        public bool Successful { get; private set; }
        public string ErrorMessage { get; private set; }


        public ContextTerminatorProcessingResult(bool success)
        {
            Successful = success;
        }


        public ContextTerminatorProcessingResult(string errorMessage)
        {
            Successful = false;
            ErrorMessage = errorMessage;
        }
    }
}
