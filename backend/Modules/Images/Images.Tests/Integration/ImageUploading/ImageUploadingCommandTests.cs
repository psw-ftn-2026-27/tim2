using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Images.Core;
using Shared.Tests;
using Xunit;

namespace Images.Tests.Integration.ImageUploading;

public sealed class ImageUploadingCommandTests : BaseIntegrationTest
{
    public ImageUploadingCommandTests(ImagesApiFactory factory) : base(factory) { }

    private static byte[] CreatePng(int length = 64)
    {
        var image = new byte[length];
        byte[] header = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52];
        header.CopyTo(image, 0);
        BinaryPrimitives.WriteInt32BigEndian(image.AsSpan(16), 800);
        BinaryPrimitives.WriteInt32BigEndian(image.AsSpan(20), 600);
        return image;
    }

    private static MultipartFormDataContent CreateForm(byte[] image, string fileName)
    {
        var file = new ByteArrayContent(image);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    private int CountStoredFiles() =>
        Directory.Exists(Factory.UploadsPath) ? Directory.GetFiles(Factory.UploadsPath, "*", SearchOption.AllDirectories).Length : 0;

    [Fact]
    public async Task Uploaded_image_is_stored_and_its_address_is_returned()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var image = CreatePng();
        using var form = CreateForm(image, "tvrdjava.png");

        var response = await client.PostAsync("/api/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UploadedImageDto>(JsonOptions);
        body!.Url.Should().StartWith("/uploads/").And.EndWith(".png");
        var storedPath = Path.Combine(Factory.WebRootPath, body.Url.TrimStart('/'));
        File.ReadAllBytes(storedPath).Should().Equal(image);
    }

    [Theory]
    [InlineData("89504E470D0A1A0A0000000D494844520000032000000258", ".png")]
    [InlineData("FFD8FFC000110802580320", ".jpg")]
    [InlineData("52494646000000005745425056503820000000000000009D012A20035802", ".webp")]
    public async Task Stored_image_gets_the_extension_of_its_format(string header, string extension)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var image = new byte[64];
        Convert.FromHexString(header).CopyTo(image, 0);
        using var form = CreateForm(image, "slika");

        var response = await client.PostAsync("/api/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UploadedImageDto>(JsonOptions);
        body!.Url.Should().EndWith(extension);
        File.Exists(Path.Combine(Factory.WebRootPath, body.Url.TrimStart('/'))).Should().BeTrue();
    }

    [Theory]
    [InlineData("tvrdjava.png")]
    [InlineData("../../tvrdjava.png")]
    [InlineData("..\\..\\tvrdjava.png")]
    public async Task Stored_address_does_not_depend_on_the_uploaded_file_name(string fileName)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = CreateForm(CreatePng(), fileName);

        var response = await client.PostAsync("/api/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UploadedImageDto>(JsonOptions);
        body!.Url.Should().MatchRegex("^/uploads/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\\.png$");
        Directory.GetFiles(Factory.UploadsPath).Select(Path.GetFileName).Should().Equal(Path.GetFileName(body.Url));
    }

    [Fact]
    public async Task Rejected_file_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = CreateForm(CreatePng(15 * 1024 * 1024 + 1), "prevelika.png");
        var storedFiles = CountStoredFiles();

        var response = await client.PostAsync("/api/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        CountStoredFiles().Should().Be(storedFiles);
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_an_image()
    {
        var client = Factory.CreateClient();
        using var form = CreateForm(CreatePng(), "tvrdjava.png");

        var response = await client.PostAsync("/api/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
