using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SizePhotos;

class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        try
        {
            await host.RunAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error encountered running application: {ex.Message}");
            Environment.Exit(1);
        }

        Environment.Exit(0);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var opts = new SizePhotoOptions();
        opts.Parse(args);

        ValidateOptions(opts);

        return Host
            .CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddPhotoProcessingServices(opts)
                    .AddHostedService<Worker>();
            });
    }

    static void ValidateOptions(SizePhotoOptions opts)
    {
        if (!Directory.Exists(opts.LocalPhotoRoot))
        {
            throw new DirectoryNotFoundException($"The picture directory specified, {opts.LocalPhotoRoot}, does not exist.  Please specify a directory containing photos.");
        }

        if (File.Exists(opts.Outfile))
        {
            throw new IOException($"The specified output file, {opts.Outfile}, already exists.  Please remove it before running this process.");
        }
    }
}
