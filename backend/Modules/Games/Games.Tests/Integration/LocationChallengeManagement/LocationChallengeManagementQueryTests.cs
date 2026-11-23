using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeManagement;
using Games.Application.Locations;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeManagement;

public class LocationChallengeManagementQueryTests : BaseIntegrationTest
{
    public LocationChallengeManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Location_challenge_details_list_the_start_and_all_hidden_points_with_their_photo_and_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.DraftOldTownSecrets;

        var challenge = await client.GetFromJsonAsync<LocationChallengeDto>($"/api/games/location-challenges/{seeded.Id}", JsonOptions);

        challenge.Should().BeEquivalentTo(
            new LocationChallengeDto(
                seeded.Id,
                seeded.Name,
                seeded.Xp,
                seeded.Status,
                new LocationDto(seeded.StartLocation.Latitude, seeded.StartLocation.Longitude),
                seeded.HiddenPoints
                    .Select(point => new HiddenPointDto(point.ImageUrl, new LocationDto(point.Location.Latitude, point.Location.Longitude)))
                    .ToList()),
            options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Details_of_an_unknown_location_challenge_are_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/games/location-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
