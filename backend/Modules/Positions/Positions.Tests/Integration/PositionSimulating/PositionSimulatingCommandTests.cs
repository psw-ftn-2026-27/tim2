using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Positions.Application.PositionSimulating;
using Positions.Infrastructure.Persistence;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Positions.Tests.Integration.PositionSimulating;

public class PositionSimulatingCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/positions/mine";

    public PositionSimulatingCommandTests(PositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_without_a_position_sets_one_and_gets_exactly_one_position()
    {
        var explorerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(explorerId, "explorer");

        var response = await client.PutAsJsonAsync(Url, new UserPositionDto(43.3209, 21.8958), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PositionsDbContext>();
        var position = assertContext.UserPositions.Should().ContainSingle(position => position.UserId == explorerId).Subject;
        position.Coordinates.Latitude.Should().Be(43.3209);
        position.Coordinates.Longitude.Should().Be(21.8958);
    }

    [Fact]
    public async Task Explorer_with_a_position_sets_it_again_and_the_same_position_moves_to_the_last_coordinates()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync(Url, new UserPositionDto(90, -180), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PositionsDbContext>();
        var position = assertContext.UserPositions.Should().ContainSingle(position => position.UserId == WellKnownUsers.Explorer).Subject;
        position.Id.Should().Be(UserPositionSeed.ExplorersPositionInNoviSad.Id);
        position.Coordinates.Latitude.Should().Be(90);
        position.Coordinates.Longitude.Should().Be(-180);
    }

    [Fact]
    public async Task Anonymous_user_cannot_set_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync(Url, new UserPositionDto(45, 19), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_set_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync(Url, new UserPositionDto(45, 19), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
