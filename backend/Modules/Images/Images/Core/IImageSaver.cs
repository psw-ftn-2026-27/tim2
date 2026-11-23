namespace Images.Core;

public interface IImageSaver
{
    Task<string> SaveAsync(ImageFile image);
}
