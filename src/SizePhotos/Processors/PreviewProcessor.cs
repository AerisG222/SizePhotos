using System;
using System.IO;
using System.Threading.Tasks;
using SizePhotos.Converters;
using SizePhotos.Exif;
using SizePhotos.Transforms;

namespace SizePhotos.Processors;

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
    readonly MetadataReader _metadataReader;

    public PreviewProcessor(
        RawTherapeeConverter rtConverter,
        PhotoResizer resizer,
        MetadataReader metadataReader
    ) {
        _rtConverter = rtConverter ?? throw new ArgumentNullException(nameof(rtConverter));
        _resizer = resizer ?? throw new ArgumentNullException(nameof(resizer));
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
    }

    public void PrepareDirectories(string sourceDirectory)
    {
        Directory.CreateDirectory(Path.Combine(sourceDirectory, _spec.OutputDirectory));
    }

    public async Task<ProcessedPhoto> ProcessAsync(string sourceFile)
    {
        var filename = $"{Path.GetFileNameWithoutExtension(sourceFile)}.tif";
        var tif = Path.Combine(Path.GetDirectoryName(sourceFile), filename);
        var exif = await _metadataReader.ReadMetadataAsync(sourceFile);

        await _rtConverter.ConvertAsync(sourceFile, tif, exif);

        var result = await _resizer.ResizePhotoAsync(tif, _spec);

        File.Delete(tif);

        return new ProcessedPhoto
        {
            Src = result
        };
    }
}
