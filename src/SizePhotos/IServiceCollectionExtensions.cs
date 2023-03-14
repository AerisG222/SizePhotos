using Microsoft.Extensions.DependencyInjection;
using SizePhotos.Exif;
using SizePhotos.Minification;
using SizePhotos.PhotoReaders;
using SizePhotos.PhotoWriters;
using SizePhotos.ResultWriters;

namespace SizePhotos;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPhotoProcessingServices(this IServiceCollection services, SizePhotoOptions opts)
    {
        return services
            .AddSingleton(opts)
            .AddPhotoProcessor(opts)
            .AddResultWriter(opts)
            .AddSingleton<PhotoResizer>()
            .AddSingleton<PhotoMinifier>()
            .AddSingleton<MetadataReader>()
            .AddSingleton<RawTherapeeConverter>();
    }

    static IServiceCollection AddPhotoProcessor(this IServiceCollection services, SizePhotoOptions opts)
    {
        if(opts.FastReview)
        {
            services.AddSingleton<IPhotoProcessor, PreviewProcessor>();
        }
        else
        {
            services.AddSingleton<IPhotoProcessor, PublishingProcessor>();
        }

        return services;
    }

    static IServiceCollection AddResultWriter(this IServiceCollection services, SizePhotoOptions opts)
    {
        if (opts.InsertMode)
        {
            services.AddSingleton<IResultWriter, PgsqlInsertResultWriter>();
        }
        else if (opts.UpdateMode)
        {
            services.AddSingleton<IResultWriter, PgsqlUpdateResultWriter>();
        }
        else
        {
            services.AddSingleton<IResultWriter, NoopResultWriter>();
        }

        return services;
    }
}
