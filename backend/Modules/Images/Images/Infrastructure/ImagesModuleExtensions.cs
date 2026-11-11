using Images.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Images.Infrastructure;

public static class ImagesModuleExtensions
{
    public static IServiceCollection AddImagesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ImageUploading>();
        services.AddScoped<IImageSaver, DiskImageSaver>();

        return services;
    }
}
