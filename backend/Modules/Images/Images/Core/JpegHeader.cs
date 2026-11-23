namespace Images.Core;

internal static class JpegHeader
{
    private static readonly byte[] Signature = [0xFF, 0xD8, 0xFF];

    private const byte MarkerPrefix = 0xFF;
    private const byte StartOfScan = 0xDA;
    private const byte EndOfImage = 0xD9;

    public static bool HasSignature(byte[] content)
    {
        return ImageBytes.StartsWith(content, 0, Signature);
    }

    // Walks the marker segments after SOI until a start-of-frame (SOFn), which holds height then width.
    public static (int Width, int Height) ReadDimensions(byte[] content)
    {
        var position = 2;
        while (position + 1 < content.Length)
        {
            if (content[position] != MarkerPrefix)
            {
                throw ImageBytes.Unreadable();
            }

            var marker = content[position + 1];
            if (IsStartOfFrame(marker))
            {
                return ReadStartOfFrame(content, position + 2);
            }

            position += SegmentSize(content, position, marker);
        }

        throw ImageBytes.Unreadable();
    }

    // How far the walk moves past the marker at position: a fill byte, a marker alone, or a marker with its segment.
    private static int SegmentSize(byte[] content, int position, byte marker)
    {
        if (marker == MarkerPrefix)
        {
            return 1;
        }

        if (IsStandaloneMarker(marker))
        {
            return 2;
        }

        return 2 + ReadSegmentLength(content, position + 2, marker);
    }

    // The two length bytes count themselves, so a valid length is at least 2.
    private static int ReadSegmentLength(byte[] content, int segmentStart, byte marker)
    {
        var reachedImageData = marker == StartOfScan || marker == EndOfImage;
        if (reachedImageData || segmentStart + 1 >= content.Length)
        {
            throw ImageBytes.Unreadable();
        }

        var length = ImageBytes.ReadBigEndian16(content, segmentStart);
        if (length < 2)
        {
            throw ImageBytes.Unreadable();
        }

        return length;
    }

    // Segment layout: length (2 bytes), precision (1 byte), height (2 bytes), width (2 bytes).
    private static (int Width, int Height) ReadStartOfFrame(byte[] content, int segmentStart)
    {
        if (segmentStart + 7 > content.Length)
        {
            throw ImageBytes.Unreadable();
        }

        var height = ImageBytes.ReadBigEndian16(content, segmentStart + 3);
        var width = ImageBytes.ReadBigEndian16(content, segmentStart + 5);
        return (width, height);
    }

    // TEM and RST0 to RST7 carry no length bytes.
    private static bool IsStandaloneMarker(byte marker)
    {
        return marker == 0x01 || (marker >= 0xD0 && marker <= 0xD7);
    }

    // C4 (DHT), C8 (JPG) and CC (DAC) share the range but are not frames.
    private static bool IsStartOfFrame(byte marker)
    {
        return marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
    }
}
