using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Locations;
using Games.Application.LocationChallengeSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public class LocationChallengeSolvingQueryTests : BaseIntegrationTest
{
    public LocationChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Progress_of_an_activated_challenge_shows_the_stored_search_area_and_every_photo_without_undiscovered_locations()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedStrandBeach;

        var progress = await client.GetFromJsonAsync<LocationChallengeProgressDto>(
            $"/api/games/location-challenges/{challenge.Id}/progress", JsonOptions);

        progress!.State.Should().Be(LocationChallengeProgressState.Active);
        progress.Name.Should().Be(challenge.Name);
        progress.Center.Should().Be(new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude));
        progress.SearchRadiusInMeters.Should().Be(LocationChallengeAttemptSeed.ThirdExplorersStrandBeachInCurrentRound.SearchRadiusInMeters);
        progress.HiddenPoints.Should().BeEquivalentTo(
            challenge.HiddenPoints.Select((point, index) => new HiddenPointProgressDto(index + 1, point.ImageUrl, null)),
            options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Progress_of_a_challenge_that_is_not_published_shows_that_it_is_unavailable()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var progress = await client.GetFromJsonAsync<LocationChallengeProgressDto>(
            $"/api/games/location-challenges/{LocationChallengeSeed.RetiredFishermansIsland.Id}/progress", JsonOptions);

        progress!.State.Should().Be(LocationChallengeProgressState.Unavailable);
        progress.HiddenPoints.Should().BeNull();
    }

    [Fact]
    public async Task Progress_of_a_player_with_an_attempt_only_from_an_earlier_round_shows_that_the_challenge_is_not_activated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var progress = await client.GetFromJsonAsync<LocationChallengeProgressDto>(
            $"/api/games/location-challenges/{LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.Id}/progress", JsonOptions);

        progress!.State.Should().Be(LocationChallengeProgressState.NotActivated);
        progress.HiddenPoints.Should().BeNull();
    }
}
