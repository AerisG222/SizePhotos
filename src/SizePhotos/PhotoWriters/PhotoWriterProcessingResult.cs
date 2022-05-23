namespace SizePhotos.PhotoWriters;

public class PhotoWriterProcessingResult
    : IProcessingResult
{
    public bool Successful { get; private set; }
    public string ScaleName { get; private set; }
    public uint Height { get; private set; }
    public uint Width { get; private set; }
    public long FileSize { get; private set; }
    public string LocalPath { get; private set; }
    public string Url { get; private set; }
    public string ErrorMessage { get; private set; }


    public PhotoWriterProcessingResult(bool success, string scaleName, uint height, uint width, long fileSize, string localPath, string url)
    {
        Successful = success;
        ScaleName = scaleName;
        Height = height;
        Width = width;
        FileSize = fileSize;
        LocalPath = localPath;
        Url = url;
    }


    public PhotoWriterProcessingResult(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
