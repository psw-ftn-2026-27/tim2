using System.Buffers.Binary;
using System.Text;
using FluentAssertions;
using Images.Core;
using Shared.Domain.Exceptions;
using Xunit;

namespace Images.Tests.Unit;

public class ImageFileTests
{
    private static byte[] CreateImage(string kind, int width, int height, int length = 64)
    {
        var header = kind switch
        {
            "png" => Concat(
                [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D],
                "IHDR"u8.ToArray(), BigEndian32(width), BigEndian32(height)),
            "jpeg" => Concat(
                [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10], "JFIF\0"u8.ToArray(), new byte[9],
                [0xFF, 0xC0, 0x00, 0x11, 0x08], BigEndian16(height), BigEndian16(width), [0x03]),
            "webp-lossy" => Concat(
                RiffHeader("VP8 "), [0x00, 0x00, 0x00, 0x9D, 0x01, 0x2A], LittleEndian16(width), LittleEndian16(height)),
            "webp-lossless" => Concat(
                RiffHeader("VP8L"), [0x2F], LittleEndian32((width - 1) | ((height - 1) << 14))),
            "webp-extended" => Concat(
                RiffHeader("VP8X"), new byte[4], LittleEndian24(width - 1), LittleEndian24(height - 1)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        var image = new byte[Math.Max(length, header.Length)];
        header.CopyTo(image, 0);
        return image;
    }

    private static byte[] RiffHeader(string chunk) =>
        Concat("RIFF"u8.ToArray(), new byte[4], "WEBP"u8.ToArray(), Encoding.ASCII.GetBytes(chunk), new byte[4]);

    private static byte[] Concat(params byte[][] parts) => parts.SelectMany(part => part).ToArray();

    private static byte[] BigEndian16(int value) => [(byte)(value >> 8), (byte)value];

    private static byte[] LittleEndian16(int value) => [(byte)value, (byte)(value >> 8)];

    private static byte[] LittleEndian24(int value) => [(byte)value, (byte)(value >> 8), (byte)(value >> 16)];

    private static byte[] BigEndian32(int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        return bytes;
    }

    private static byte[] LittleEndian32(int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        return bytes;
    }

    [Fact]
    public void File_larger_than_15MB_is_rejected()
    {
        var content = CreateImage("png", 800, 600, 15 * 1024 * 1024 + 1);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"></svg>")]
    [InlineData("<?xml version=\"1.0\"?><svg xmlns=\"http://www.w3.org/2000/svg\"></svg>")]
    [InlineData("GIF89a\u0001\u0000\u0001\u0000")]
    [InlineData("RIFF\u0000\u0000\u0000\u0000WAVEfmt ")]
    [InlineData("Obican tekst sacuvan pod imenom slika.png")]
    [InlineData("")]
    public void File_whose_content_is_not_jpeg_png_or_webp_is_rejected(string text)
    {
        var content = Encoding.UTF8.GetBytes(text);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("jpeg", ImageFormat.Jpeg)]
    [InlineData("png", ImageFormat.Png)]
    [InlineData("webp-lossy", ImageFormat.WebP)]
    [InlineData("webp-lossless", ImageFormat.WebP)]
    [InlineData("webp-extended", ImageFormat.WebP)]
    public void Content_in_an_allowed_format_is_recognised_as_that_format(string kind, ImageFormat format)
    {
        var content = CreateImage(kind, 800, 600);

        var image = new ImageFile(content);

        image.Format.Should().Be(format);
        image.Content.Should().Equal(content);
    }

    [Theory]
    [InlineData("jpeg", 10001, 600)]
    [InlineData("jpeg", 800, 10001)]
    [InlineData("png", 10001, 600)]
    [InlineData("png", 800, 10001)]
    [InlineData("webp-lossy", 10001, 600)]
    [InlineData("webp-lossless", 800, 10001)]
    [InlineData("webp-extended", 10001, 600)]
    [InlineData("webp-extended", 800, 10001)]
    public void Image_with_too_large_dimensions_is_rejected(string kind, int width, int height)
    {
        var content = CreateImage(kind, width, height);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }
}
