namespace Images.Core;

internal static class WebPHeader
{
    public static bool HasSignature(byte[] content)
    {
        return ImageBytes.StartsWith(content, 0, "RIFF"u8.ToArray())
            && ImageBytes.StartsWith(content, 8, "WEBP"u8.ToArray());
    }

    // The first chunk after the RIFF header decides the layout: lossy (VP8), lossless (VP8L) or extended (VP8X).
    public static (int Width, int Height) ReadDimensions(byte[] content)
    {
        if (ImageBytes.StartsWith(content, 12, "VP8 "u8.ToArray()))
        {
            return ReadLossyDimensions(content);
        }

        if (ImageBytes.StartsWith(content, 12, "VP8L"u8.ToArray()))
        {
            return ReadLosslessDimensions(content);
        }

        if (ImageBytes.StartsWith(content, 12, "VP8X"u8.ToArray()))
        {
            return ReadExtendedDimensions(content);
        }

        throw ImageBytes.Unreadable();
    }

    // A key frame starts with the tag 9D 01 2A, followed by 14-bit little-endian width and height.
    private static (int Width, int Height) ReadLossyDimensions(byte[] content)
    {
        if (content.Length < 30 || !ImageBytes.StartsWith(content, 23, [0x9D, 0x01, 0x2A]))
        {
            throw ImageBytes.Unreadable();
        }

        var width = (content[26] | (content[27] << 8)) & 0x3FFF;
        var height = (content[28] | (content[29] << 8)) & 0x3FFF;
        return (width, height);
    }

    // After the signature byte 2F, width - 1 and height - 1 are packed into 14 bits each.
    private static (int Width, int Height) ReadLosslessDimensions(byte[] content)
    {
        if (content.Length < 25 || content[20] != 0x2F)
        {
            throw ImageBytes.Unreadable();
        }

        var width = 1 + (content[21] | ((content[22] & 0x3F) << 8));
        var height = 1 + ((content[22] >> 6) | (content[23] << 2) | ((content[24] & 0x0F) << 10));
        return (width, height);
    }

    // After four bytes of flags, width - 1 and height - 1 are 24-bit little-endian.
    private static (int Width, int Height) ReadExtendedDimensions(byte[] content)
    {
        if (content.Length < 30)
        {
            throw ImageBytes.Unreadable();
        }

        var width = 1 + (content[24] | (content[25] << 8) | (content[26] << 16));
        var height = 1 + (content[27] | (content[28] << 8) | (content[29] << 16));
        return (width, height);
    }
}
