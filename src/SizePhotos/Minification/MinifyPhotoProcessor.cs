using System;
using System.IO;
using System.Threading.Tasks;
using NJpegOptim;
using NJpegTran;
using SizePhotos.PhotoWriters;


namespace SizePhotos.Minification;

public class MinifyPhotoProcessor
    : IPhotoProcessor
{
    readonly string _scaleName;
    readonly short _jpgQuality;
    readonly PhotoPathHelper _pathHelper;


    public MinifyPhotoProcessor(string scaleName, short jpgQuality, PhotoPathHelper pathHelper)
    {
        if (string.IsNullOrEmpty(scaleName))
        {
            throw new ArgumentNullException(nameof(scaleName));
        }

        if (jpgQuality < 1 || jpgQuality > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(jpgQuality), "Quality must be in the range [0, 100]");
        }

        _scaleName = scaleName;
        _jpgQuality = jpgQuality;
        _pathHelper = pathHelper ?? throw new ArgumentNullException(nameof(pathHelper));
    }


    public IPhotoProcessor Clone()
    {
        return (IPhotoProcessor)MemberwiseClone();
    }


    public async Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext context)
    {
        NJpegOptim.Result optimResult = null;
        NJpegTran.Result tranResult = null;
        var file = context.GetPhotoWriterResult(_scaleName).LocalPath;

        try
        {
            optimResult = await ExecuteJpegOptim(file);

            if (!optimResult.Success)
            {
                return new MinifyProcessingResult("Error executing jpegoptim, so the file was not minified.");
            }

            tranResult = await ExecuteJpegTran(optimResult.OutputStream, file);

            if (!tranResult.Success)
            {
                return new MinifyProcessingResult("Error executing jpegtran, so the file was not minified.");
            }
        }
        catch (Exception ex)
        {
            return new MinifyProcessingResult($"Error minifying file {file}: {ex.Message}");
        }
        finally
        {
            optimResult?.OutputStream?.Dispose();
            tranResult?.OutputStream?.Dispose();
        }

        return new MinifyProcessingResult(true);
    }


    async Task<NJpegOptim.Result> ExecuteJpegOptim(string srcPath)
    {
        var opts = new NJpegOptim.Options
        {
            StripProperties = StripProperty.All,
            ProgressiveMode = ProgressiveMode.ForceProgressive,
            MaxQuality = _jpgQuality,
            OutputToStream = true
        };

        var jo = new JpegOptim(opts);

        return await jo.RunAsync(srcPath);
    }


    async Task<NJpegTran.Result> ExecuteJpegTran(Stream inputStream, string dstPath)
    {
        var opts = new NJpegTran.Options
        {
            Optimize = true,
            Copy = Copy.None
        };

        var jt = new JpegTran(opts);

        return await jt.RunAsync(inputStream, dstPath);
    }
}
