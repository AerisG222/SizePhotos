namespace SizePhotos
{
    public class MoveProcessingResult
        : IProcessingResult
    {
        public bool Successful { get; private set; }


        public MoveProcessingResult(bool success)
        {
            Successful = success;
        }
    }
}
