namespace SizePhotos.PhotoWriters;

public class ResizeResult
{
    public MawSize Size { get; init; }
    public string OutputFile { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public long SizeInBytes { get; init; }
}
