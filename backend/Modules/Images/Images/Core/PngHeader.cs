namespace Images.Core;

internal static class PngHeader
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static bool HasSignature(byte[] content)
    {
        return ImageBytes.StartsWith(content, 0, Signature);
    }

    // IHDR is always the first chunk: width and height are big-endian at offsets 16 and 20.
    public static (int Width, int Height) ReadDimensions(byte[] content)
    {
        if (content.Length < 24 || !ImageBytes.StartsWith(content, 12, "IHDR"u8.ToArray()))
        {
            throw ImageBytes.Unreadable();
        }

        return (ImageBytes.ReadBigEndian32(content, 16), ImageBytes.ReadBigEndian32(content, 20));
    }
}
