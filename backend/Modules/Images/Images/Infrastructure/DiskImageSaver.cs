using Images.Core;
using Microsoft.AspNetCore.Hosting;

namespace Images.Infrastructure;

internal sealed class DiskImageSaver : IImageSaver
{
    private const string UploadsFolder = "uploads";

    private readonly IWebHostEnvironment _environment;

    public DiskImageSaver(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(ImageFile image)
    {
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, UploadsFolder);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{Extension(image.Format)}";
        await File.WriteAllBytesAsync(Path.Combine(folder, fileName), image.Content);

        return $"/{UploadsFolder}/{fileName}";
    }

    private static string Extension(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => ".jpg",
            ImageFormat.Png => ".png",
            _ => ".webp"
        };
    }
}
