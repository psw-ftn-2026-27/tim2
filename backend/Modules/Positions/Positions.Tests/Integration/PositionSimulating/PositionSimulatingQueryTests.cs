using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Positions.Application.UserPositions;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Positions.Tests.Integration.PositionSimulating;

public class PositionSimulatingQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/positions/mine";

    public PositionSimulatingQueryTests(PositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_gets_their_own_position_and_not_someone_elses()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var position = await client.GetFromJsonAsync<UserPositionDto>(Url, JsonOptions);

        var seeded = UserPositionSeed.ExplorersPositionInNoviSad.Coordinates;
        position.Should().Be(new UserPositionDto(seeded.Latitude, seeded.Longitude));
    }

    [Fact]
    public async Task Explorer_without_a_position_gets_an_empty_response()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.GetAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }
}
