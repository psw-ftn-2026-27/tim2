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
        if (width > MaxDimensionInPixels || height > MaxDimensionInPixels)
        {
            throw new DomainException($"An image must not be wider or taller than {MaxDimensionInPixels} pixels.");
        }

        Content = content;
    }

    private static ImageFormat DetectFormat(byte[] content)
    {
        if (StartsWith(content, 0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return ImageFormat.Png;
        }

        if (StartsWith(content, 0, [0xFF, 0xD8, 0xFF]))
        {
            return ImageFormat.Jpeg;
        }

        if (StartsWith(content, 0, "RIFF"u8.ToArray()) && StartsWith(content, 8, "WEBP"u8.ToArray()))
        {
            return ImageFormat.WebP;
        }

        throw new DomainException("An image must be a JPEG, PNG or WebP file.");
    }

    private static (int Width, int Height) ReadDimensions(byte[] content, ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Png => ReadPngDimensions(content),
            ImageFormat.Jpeg => ReadJpegDimensions(content),
            _ => ReadWebPDimensions(content)
        };
    }

    // IHDR is always the first chunk: width and height are big-endian at offsets 16 and 20.
    private static (int Width, int Height) ReadPngDimensions(byte[] content)
    {
        if (content.Length < 24 || !StartsWith(content, 12, "IHDR"u8.ToArray()))
        {
            throw Unreadable();
        }

        return (ReadBigEndian32(content, 16), ReadBigEndian32(content, 20));
    }

    // Walks the marker segments until a start-of-frame (SOFn), which holds height then width.
    private static (int Width, int Height) ReadJpegDimensions(byte[] content)
    {
        var position = 2;
        while (position + 1 < content.Length)
        {
            if (content[position] != 0xFF)
            {
                throw Unreadable();
            }

            var marker = content[position + 1];
            position += 2;

            if (marker == 0xFF)
            {
                position--;
                continue;
            }

            if (marker == 0x01 || (marker >= 0xD0 && marker <= 0xD7))
            {
                continue;
            }

            if (marker == 0xD9 || marker == 0xDA || position + 1 >= content.Length)
            {
                throw Unreadable();
            }

            var segmentLength = (content[position] << 8) | content[position + 1];
            if (IsStartOfFrame(marker))
            {
                if (position + 7 > content.Length)
                {
                    throw Unreadable();
                }

                var height = (content[position + 3] << 8) | content[position + 4];
                var width = (content[position + 5] << 8) | content[position + 6];
                return (width, height);
            }

            if (segmentLength < 2)
            {
                throw Unreadable();
            }

            position += segmentLength;
        }

        throw Unreadable();
    }

    private static bool IsStartOfFrame(byte marker)
    {
        return marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
    }

    // The first chunk after the RIFF header decides the layout: lossy (VP8), lossless (VP8L) or extended (VP8X).
    private static (int Width, int Height) ReadWebPDimensions(byte[] content)
    {
        if (StartsWith(content, 12, "VP8 "u8.ToArray()) && content.Length >= 30
            && StartsWith(content, 23, [0x9D, 0x01, 0x2A]))
        {
            var width = (content[26] | (content[27] << 8)) & 0x3FFF;
            var height = (content[28] | (content[29] << 8)) & 0x3FFF;
            return (width, height);
        }

        if (StartsWith(content, 12, "VP8L"u8.ToArray()) && content.Length >= 25 && content[20] == 0x2F)
        {
            var width = 1 + (content[21] | ((content[22] & 0x3F) << 8));
            var height = 1 + ((content[22] >> 6) | (content[23] << 2) | ((content[24] & 0x0F) << 10));
            return (width, height);
        }

        if (StartsWith(content, 12, "VP8X"u8.ToArray()) && content.Length >= 30)
        {
            var width = 1 + (content[24] | (content[25] << 8) | (content[26] << 16));
            var height = 1 + (content[27] | (content[28] << 8) | (content[29] << 16));
            return (width, height);
        }

        throw Unreadable();
    }

    private static bool StartsWith(byte[] content, int offset, byte[] expected)
    {
        return content.Length >= offset + expected.Length
            && content.AsSpan(offset, expected.Length).SequenceEqual(expected);
    }

    private static int ReadBigEndian32(byte[] content, int offset)
    {
        var value = ((uint)content[offset] << 24) | ((uint)content[offset + 1] << 16)
            | ((uint)content[offset + 2] << 8) | content[offset + 3];
        return value > int.MaxValue ? int.MaxValue : (int)value;
    }

    private static DomainException Unreadable()
    {
        return new DomainException("The file content is not a valid image.");
    }
}
