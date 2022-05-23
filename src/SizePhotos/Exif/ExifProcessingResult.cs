namespace SizePhotos.Exif;

public class ExifProcessingResult
    : IProcessingResult
{
    public bool Successful { get; private set; }
    public ExifData ExifData { get; private set; }
    public string ErrorMessage { get; private set; }

    public ExifProcessingResult(bool success, ExifData data)
    {
        Successful = success;
        ExifData = data;
    }

    public ExifProcessingResult(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
