using Shared.Domain.Exceptions;

namespace Images.Core;

// Byte-level helpers shared by the three format headers.
internal static class ImageBytes
{
    public static bool StartsWith(byte[] content, int offset, byte[] expected)
    {
        return content.Length >= offset + expected.Length
            && content.AsSpan(offset, expected.Length).SequenceEqual(expected);
    }

    public static int ReadBigEndian16(byte[] content, int offset)
    {
        return (content[offset] << 8) | content[offset + 1];
    }

    public static int ReadBigEndian32(byte[] content, int offset)
    {
        var value = ((uint)content[offset] << 24) | ((uint)content[offset + 1] << 16)
            | ((uint)content[offset + 2] << 8) | content[offset + 3];
        return value > int.MaxValue ? int.MaxValue : (int)value;
    }

    public static DomainException Unreadable()
    {
        return new DomainException("The file content is not a valid image.");
    }
}
