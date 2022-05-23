namespace SizePhotos.Minification;

public class MinifyProcessingResult
    : IProcessingResult
{
    public bool Successful { get; private set; }
    public string ErrorMessage { get; private set; }


    public MinifyProcessingResult(bool success)
    {
        Successful = success;
    }


    public MinifyProcessingResult(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
