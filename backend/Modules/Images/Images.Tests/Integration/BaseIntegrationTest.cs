using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Shared.Tests;
using Xunit;

namespace Images.Tests.Integration;

/// <summary>
/// Points the host's web root at a temporary folder of its own, so uploaded files never
/// land in Host.Api/wwwroot and disappear with the factory.
/// </summary>
public sealed class ImagesApiFactory : ExplorerApiFactory
{
    public string WebRootPath { get; } = Path.Combine(Path.GetTempPath(), "explorer-images-tests", Guid.NewGuid().ToString());

    public string UploadsPath => Path.Combine(WebRootPath, "uploads");

    /// <summary>
    /// Restores the uploads folder to its empty baseline. Called from the test base class
    /// before every test, the disk counterpart of Reseed.
    /// </summary>
    public void ClearUploads()
    {
        if (Directory.Exists(UploadsPath))
        {
            Directory.Delete(UploadsPath, recursive: true);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        Directory.CreateDirectory(WebRootPath);
        builder.UseWebRoot(WebRootPath);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (Directory.Exists(WebRootPath))
        {
            Directory.Delete(WebRootPath, recursive: true);
        }
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<ImagesApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly ImagesApiFactory Factory;

    protected BaseIntegrationTest(ImagesApiFactory factory)
    {
        Factory = factory;
        Factory.ClearUploads();
    }
}
