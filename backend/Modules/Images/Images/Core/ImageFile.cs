using Shared.Domain.Exceptions;

namespace Images.Core;

public sealed record ImageFile
{
    public const int MaxSizeInBytes = 15 * 1024 * 1024;
    public const int MaxDimensionInPixels = 10000;

    public byte[] Content { get; }
    public ImageFormat Format { get; }

    public ImageFile(byte[] content)
    {
        if (content.Length > MaxSizeInBytes)
        {
            throw new DomainException("An image must not be larger than 15MB.");
        }

        Format = DetectFormat(content);

        var (width, height) = ReadDimensions(content, Format);
        EnsureAllowedDimensions(width, height);

        Content = content;
    }

    private static void EnsureAllowedDimensions(int width, int height)
    {
        if (width < 1 || height < 1)
        {
            throw new DomainException("An image must be at least one pixel wide and tall.");
        }

        if (width > MaxDimensionInPixels || height > MaxDimensionInPixels)
        {
            throw new DomainException($"An image must not be wider or taller than {MaxDimensionInPixels} pixels.");
        }
    }

    private static ImageFormat DetectFormat(byte[] content)
    {
        if (PngHeader.HasSignature(content))
        {
            return ImageFormat.Png;
        }

        if (JpegHeader.HasSignature(content))
        {
            return ImageFormat.Jpeg;
        }

        if (WebPHeader.HasSignature(content))
        {
            return ImageFormat.WebP;
        }

        throw new DomainException("An image must be a JPEG, PNG or WebP file.");
    }

    private static (int Width, int Height) ReadDimensions(byte[] content, ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Png => PngHeader.ReadDimensions(content),
            ImageFormat.Jpeg => JpegHeader.ReadDimensions(content),
            _ => WebPHeader.ReadDimensions(content)
        };
    }
}
