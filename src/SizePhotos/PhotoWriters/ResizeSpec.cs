namespace SizePhotos.PhotoWriters;

public class ResizeSpec
{
    public MawSize Size { get; init; }
    public int Height { get; init; }
    public int Width { get; init; }
    public ResizeMode Mode { get; init; }
    public string OutputDirectory { get; init; }
}
