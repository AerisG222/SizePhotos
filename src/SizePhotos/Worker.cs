using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SizePhotos.ResultWriters;

namespace SizePhotos;

public class Worker
    : BackgroundService
{
    static readonly string[] PHOTO_EXTENSIONS = { ".jpg", ".nef" };
    readonly SizePhotoOptions _opts;
    readonly IPhotoProcessor _processor;
    readonly IResultWriter _resultWriter;

    public Worker(SizePhotoOptions opts, IPhotoProcessor processor, IResultWriter resultWriter)
    {
        _opts = opts ?? throw new ArgumentNullException(nameof(opts));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _resultWriter = resultWriter ?? throw new ArgumentNullException(nameof(resultWriter));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return ResizePhotosAsync(stoppingToken);
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
            Console.WriteLine($"Processing: {Path.GetFileName(file)}");
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
