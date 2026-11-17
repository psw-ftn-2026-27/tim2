using System.Text.Json;
using System.Text.Json.Serialization;
using Positions.Infrastructure.Persistence;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Positions.Tests.Integration;

public sealed class PositionsApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<PositionsApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly PositionsApiFactory Factory;

    protected BaseIntegrationTest(PositionsApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<PositionsDbContext>(PositionsSeed.All);
    }
}
