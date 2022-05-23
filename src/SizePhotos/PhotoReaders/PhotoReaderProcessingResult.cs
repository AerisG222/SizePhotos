namespace SizePhotos.PhotoReaders;

public class PhotoReaderProcessingResult
    : IProcessingResult
{
    public bool Successful { get; private set; }
    public bool Skipped { get; private set; }
    public uint Height { get; private set; }
    public uint Width { get; private set; }
    public long FileSize { get; private set; }
    public string Url { get; private set; }
    public string ErrorMessage { get; private set; }


    public PhotoReaderProcessingResult(bool success, bool skipped)
    {
        Successful = success;
        Skipped = skipped;
    }


    public PhotoReaderProcessingResult(bool success, bool skipped, uint height, uint width, long fileSize, string url)
        : this(success, skipped)
    {
        Height = height;
        Width = width;
        Url = url;
        FileSize = fileSize;
    }


    public PhotoReaderProcessingResult(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
