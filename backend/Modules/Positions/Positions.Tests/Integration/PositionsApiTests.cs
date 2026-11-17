using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Positions.Contracts;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Positions.Tests.Integration;

public sealed class PositionsApiTests : BaseIntegrationTest
{
    public PositionsApiTests(PositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Another_module_gets_the_last_position_of_the_given_explorer()
    {
        using var scope = Factory.Services.CreateScope();
        var positionsApi = scope.ServiceProvider.GetRequiredService<IPositionsApi>();

        var position = await positionsApi.GetPositionAsync(WellKnownUsers.SecondExplorer);

        var seeded = UserPositionSeed.SecondExplorersPositionInBelgrade.Coordinates;
        position.Should().Be(new PositionSummary(seeded.Latitude, seeded.Longitude));
    }

    [Fact]
    public async Task Another_module_gets_nothing_for_a_user_without_a_position()
    {
        using var scope = Factory.Services.CreateScope();
        var positionsApi = scope.ServiceProvider.GetRequiredService<IPositionsApi>();

        var position = await positionsApi.GetPositionAsync(Guid.NewGuid());

        position.Should().BeNull();
    }
}
