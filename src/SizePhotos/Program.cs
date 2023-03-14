using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SizePhotos;

class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var log = host.Services.GetRequiredService<ILogger<Program>>();
        var sw = new Stopwatch();
        try
        {
            log.LogInformation("Starting to process photos at {Time}", DateTime.Now);

            sw.Start();
            await host.RunAsync();
            sw.Stop();
        }
        catch(Exception ex)
        {

            log.LogError(ex, "Error encountered running application: {Error}", ex.Message);
            Environment.Exit(1);
        }

        log.LogInformation("Completed processing photos, took {Seconds} seconds", sw.Elapsed.TotalSeconds);

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
