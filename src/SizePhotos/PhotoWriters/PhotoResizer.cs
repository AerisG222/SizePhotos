using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace SizePhotos.PhotoWriters;

public class PhotoResizer
{
    public async Task<IEnumerable<ResizeResult>> ResizePhotoAsync(string srcFile, IEnumerable<ResizeSpec> specs)
    {
        var results = new List<ResizeResult>();
        using var image = Image.Load(srcFile);

        foreach(var spec in specs)
        {
            using var copy = image.Clone(ctx => Resize(ctx, spec));
            //StripMetadata(copy);

            var outputFile = GetOutputFilename(srcFile, spec);

            await copy.SaveAsJpegAsync(outputFile);

            results.Add(BuildResult(spec, copy, outputFile));
        }

        return results;
    }

    public async Task<ResizeResult> ResizePhotoAsync(string srcFile, ResizeSpec spec)
    {
        using var image = Image.Load(srcFile);

        image.Mutate(ctx => Resize(ctx, spec));
        //StripMetadata(image);

        var outputFile = GetOutputFilename(srcFile, spec);

        await image.SaveAsJpegAsync(outputFile);

        return BuildResult(spec, image, outputFile);
    }

    string GetOutputFilename(string srcFile, ResizeSpec spec)
    {
        var outdir = Path.Combine(Path.GetDirectoryName(srcFile), spec.OutputDirectory);
        var filename = $"{Path.GetFileNameWithoutExtension(srcFile)}.jpg";

        return Path.Combine(outdir, filename);
    }

    void StripMetadata(Image image)
    {
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;
    }

    IImageProcessingContext Resize(IImageProcessingContext ctx, ResizeSpec spec)
    {
        if(spec.Mode == ResizeMode.None)
        {
            return ctx;
        }

        var opts = new ResizeOptions()
        {
            Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
            Size = new Size(spec.Width, spec.Height),
            Sampler = LanczosResampler.Lanczos3
        };

        ctx.Resize(opts);

        if(spec.Mode == ResizeMode.Fixed)
        {
            ctx.Crop(spec.Width, spec.Height);
        }

        return ctx.GaussianSharpen(0.6f);
    }

    ResizeResult BuildResult(ResizeSpec spec, Image image, string outputFile)
    {
        return new ResizeResult
        {
            Size = spec.Size,
            OutputFile = outputFile,
            Width = image.Width,
            Height = image.Height,
            SizeInBytes = new FileInfo(outputFile).Length
        };
    }
}
