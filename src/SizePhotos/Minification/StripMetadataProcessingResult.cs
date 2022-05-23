namespace SizePhotos.Minification;

public class StripMetadataProcessingResult
    : IProcessingResult
{
    public bool Successful { get; private set; }
    public string ErrorMessage { get; private set; }


    public StripMetadataProcessingResult(bool success)
    {
        Successful = success;
    }


    public StripMetadataProcessingResult(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
