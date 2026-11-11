namespace Images.Core;

public sealed class ImageUploading
{
    private readonly IImageSaver _imageSaver;

    public ImageUploading(IImageSaver imageSaver)
    {
        _imageSaver = imageSaver;
    }

    public async Task<UploadedImageDto> UploadAsync(byte[] content)
    {
        var image = new ImageFile(content);

        var url = await _imageSaver.SaveAsync(image);

        return new UploadedImageDto(url);
    }
}
