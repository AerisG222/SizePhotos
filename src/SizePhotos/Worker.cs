using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SizePhotos.ResultWriters;

namespace SizePhotos;

public class Worker
    : BackgroundService
{
    static readonly string[] PHOTO_EXTENSIONS = { ".jpg", ".nef" };
    readonly SizePhotoOptions _opts;
    readonly IPhotoProcessor _processor;
    readonly ILogger _log;
    readonly IResultWriter _resultWriter;
    readonly IHostApplicationLifetime _appLifetime;

    public Worker(
        IHostApplicationLifetime appLifetime,
        ILogger<Worker> log,
        SizePhotoOptions opts,
        IPhotoProcessor processor,
        IResultWriter resultWriter
    ) {
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _opts = opts ?? throw new ArgumentNullException(nameof(opts));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _resultWriter = resultWriter ?? throw new ArgumentNullException(nameof(resultWriter));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ResizePhotosAsync(stoppingToken);

        _appLifetime.StopApplication();
    }

    async Task ResizePhotosAsync(CancellationToken stoppingToken)
    {
        if(!_opts.FastReview)
        {
            MoveSourceFiles();
        }

        var files = GetPhotos().ToArray();
        var parallelOpts = new ParallelOptions
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

        // seeing we know how many items there are, allocate an array for results to be written to
        // this avoids the need for locking and also guarantees result order (though not important here)
        var results = new ProcessedPhoto[files.Length];

        _processor.PrepareDirectories(_opts.LocalPhotoRoot);

        await Parallel.ForEachAsync(Enumerable.Range(0, files.Length), parallelOpts, async (index, token) =>
        {
            var file = files[index];

            results[index] = await ProcessPhotoAsync(file);
        });

        _resultWriter.WriteOutput(_opts.Outfile, _opts.CategoryInfo, results);
    }

    Task<ProcessedPhoto> ProcessPhotoAsync(string file)
    {
        if (!_opts.Quiet)
        {
            _log.LogInformation("Processing: {Path}", Path.GetFileName(file));
        }

        return _processor.ProcessAsync(file);
    }

    IEnumerable<string> GetPhotos()
    {
        return Directory.EnumerateFiles(_opts.LocalPhotoRoot)
            .Where(x => PHOTO_EXTENSIONS.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    void MoveSourceFiles()
    {
        var newRoot = Path.Combine(_opts.LocalPhotoRoot, "src");

        Directory.CreateDirectory(newRoot);

        foreach(var file in Directory.EnumerateFiles(_opts.LocalPhotoRoot))
        {
            File.Move(file, Path.Combine(newRoot));
        }

        _opts.ResetLocalRoot(newRoot);
    }
}
