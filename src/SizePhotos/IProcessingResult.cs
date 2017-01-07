namespace SizePhotos
{
    public interface IProcessingResult
    {
        bool Successful { get; }
        string ErrorMessage { get; }
    }
}
