using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SizePhotos.Exif;
using SizePhotos.Minification;
using SizePhotos.PhotoReaders;
using SizePhotos.PhotoWriters;

namespace SizePhotos;

public class PublishingProcessor
    : IPhotoProcessor
{
    const string DIR_SRC = "src";

    static readonly ResizeSpec[] _specs = new ResizeSpec[]
    {
        new ResizeSpec
        {
            Size = MawSize.XsSq,
            Mode = ResizeMode.Fixed,
            Width = 160,
            Height = 120,
            OutputDirectory = "xs_sq"
        },
        new ResizeSpec
        {
            Size = MawSize.Xs,
            Mode = ResizeMode.AspectToWidth,
            Width = 160,
            Height = 120,
            OutputDirectory = "xs"
        },
        new ResizeSpec
        {
            Size = MawSize.Sm,
            Mode = ResizeMode.AspectToWidth,
            Width = 640,
            Height = 480,
            OutputDirectory = "sm"
        },
        new ResizeSpec
        {
            Size = MawSize.Md,
            Mode = ResizeMode.AspectToWidth,
            Width = 1024,
            Height = 768,
            OutputDirectory = "md"
        },
        new ResizeSpec
        {
            Size = MawSize.Lg,
            Mode = ResizeMode.None,
            Width = 0,
            Height = 0,
            OutputDirectory = "lg"
        },
        new ResizeSpec
        {
            Size = MawSize.Prt,
            Mode = ResizeMode.None,
            Width = 0,
            Height = 0,
            OutputDirectory = "prt"
        }
    };

    readonly RawTherapeeConverter _rtConverter;
    readonly PhotoResizer _resizer;
    readonly PhotoMinifier _minifier;
    readonly MetadataReader _metadataReader;

    public PublishingProcessor(
        RawTherapeeConverter rtConverter,
        PhotoResizer resizer,
        PhotoMinifier minifier,
        MetadataReader metadataReader
    ) {
        _rtConverter = rtConverter ?? throw new ArgumentNullException(nameof(rtConverter));
        _resizer = resizer ?? throw new ArgumentNullException(nameof(resizer));
        _minifier = minifier ?? throw new ArgumentNullException(nameof(minifier));
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
    }

    public void PrepareDirectories(string sourceDirectory)
    {
        var newSrc = Path.Combine(sourceDirectory, DIR_SRC);

        if(!Directory.Exists(newSrc))
        {
            Directory.CreateDirectory(newSrc);
        }

        foreach(var spec in _specs)
        {
            Directory.CreateDirectory(Path.Combine(sourceDirectory, spec.OutputDirectory));
        }
    }

    public async Task<ProcessedPhoto> ProcessAsync(string sourceFile)
    {
        var tifFilename = $"{Path.GetFileNameWithoutExtension(sourceFile)}.tif";
        var tifPath = Path.Combine(Path.GetDirectoryName(sourceFile), tifFilename);

        var exif = await _metadataReader.ReadMetadataAsync(sourceFile);

        await _rtConverter.ConvertAsync(sourceFile, tifPath);

        var resizeResult = await _resizer.ResizePhotoAsync(tifPath, _specs);

        var filesToMinify = resizeResult.Select(x => x.OutputFile);

        await _minifier.MinifyPhotosAsync(filesToMinify, 72);

        var prt = resizeResult.Single(x => x.Size == MawSize.Prt);

        var src = MoveFileToFinalSourceDirectory(sourceFile);

        return new ProcessedPhoto
        {
            ExifData = exif,
            XsSq = resizeResult.Single(x => x.Size == MawSize.XsSq),
            Xs = resizeResult.Single(x => x.Size == MawSize.Xs),
            Sm = resizeResult.Single(x => x.Size == MawSize.Sm),
            Md = resizeResult.Single(x => x.Size == MawSize.Md),
            Lg = resizeResult.Single(x => x.Size == MawSize.Lg),
            Prt = prt,
            Src = new ResizeResult
            {
                Size = MawSize.Src,
                OutputFile = src,
                Width = prt.Width,
                Height = prt.Height,
                SizeInBytes = new FileInfo(src).Length
            }
        };
    }

    string MoveFileToFinalSourceDirectory(string file)
    {
        var srcDir = Path.Combine(Path.GetDirectoryName(file), DIR_SRC);

        File.Move(file, srcDir);

        var pp3 = $"{file}.pp3";

        if(File.Exists(pp3))
        {
            File.Move(pp3, srcDir);
        }

        return Path.Combine(srcDir, Path.GetFileName(file));
    }
}