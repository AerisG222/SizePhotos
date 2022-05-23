namespace SizePhotos;

public class MoveProcessingResult
    : IProcessingResult
{
    public bool Successful { get; private set; }
    public string ErrorMessage { get; private set; }


    public MoveProcessingResult(bool success)
    {
        Successful = success;
    }


    public MoveProcessingResult(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
