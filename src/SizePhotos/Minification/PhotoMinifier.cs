using System.Collections.Generic;
using System.Threading.Tasks;
using NJpegOptim;
using NJpegTran;

namespace SizePhotos.Minification;

public class PhotoMinifier
{
    public async Task MinifyPhotosAsync(IEnumerable<string> files, short jpgQuality)
    {
        foreach(var file in files)
        {
            await MinifyPhotoAsync(file, jpgQuality);
        }
    }

    public async Task MinifyPhotoAsync(string file, short jpgQuality)
    {
        await ExecuteJpegOptim(file, jpgQuality);
        await ExecuteJpegTran(file);
    }

    async Task ExecuteJpegOptim(string file, short jpgQuality)
    {
        var opts = new NJpegOptim.Options
        {
            StripProperties = StripProperty.All,
            ProgressiveMode = ProgressiveMode.ForceProgressive,
            MaxQuality = jpgQuality
        };

        var jo = new JpegOptim(opts);

        await jo.RunAsync(file);
    }

    async Task ExecuteJpegTran(string file)
    {
        var opts = new NJpegTran.Options
        {
            Optimize = true,
            Copy = Copy.None
        };

        var jt = new JpegTran(opts);

        await jt.RunAsync(file);
    }
}
