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
        // Static files are served only from a web root that existed at startup, so a file saved
        // into a folder created now would get an address that answers 404 until a restart.
        var webRoot = _environment.WebRootPath
            ?? throw new InvalidOperationException("The web root folder (wwwroot) did not exist when the application started.");
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
