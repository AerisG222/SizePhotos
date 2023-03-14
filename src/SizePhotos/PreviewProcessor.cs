using System;
using System.IO;
using System.Threading.Tasks;
using SizePhotos.PhotoReaders;
using SizePhotos.PhotoWriters;

namespace SizePhotos;

public class PreviewProcessor
    : IPhotoProcessor
{
    static readonly ResizeSpec _spec = new ResizeSpec
    {
        Size = MawSize.Review,
        Mode = ResizeMode.None,
        Width = 0,
        Height = 0,
        OutputDirectory = "review"
    };

    readonly RawTherapeeConverter _rtConverter;
    readonly PhotoResizer _resizer;

    public PreviewProcessor(
        RawTherapeeConverter rtConverter,
        PhotoResizer resizer
    ) {
        _rtConverter = rtConverter ?? throw new ArgumentNullException(nameof(rtConverter));
        _resizer = resizer ?? throw new ArgumentNullException(nameof(resizer));
    }

    public void PrepareDirectories(string sourceDirectory)
    {
        Directory.CreateDirectory(Path.Combine(sourceDirectory, _spec.OutputDirectory));
    }

    public async Task<ProcessedPhoto> ProcessAsync(string sourceFile)
    {
        var filename = $"{Path.GetFileNameWithoutExtension(sourceFile)}.tif";
        var tif = Path.Combine(Path.GetDirectoryName(sourceFile), filename);

        await _rtConverter.ConvertAsync(sourceFile, tif);

        var result = await _resizer.ResizePhotoAsync(tif, _spec);

        File.Delete(tif);

        return new ProcessedPhoto
        {
            Src = result
        };
    }
}
