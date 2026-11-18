using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeManagement;
using Games.Application.Locations;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeManagement;

public class LocationChallengeManagementCommandTests : BaseIntegrationTest
{
    public LocationChallengeManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    private static CreateLocationChallengeDto CreateRequest() => new(
        "Bedemi Petrovaradinske tvrđave",
        150,
        new LocationDto(45.2520, 19.8620),
        [
            new HiddenPointDto("/uploads/bedem-sat.jpg", new LocationDto(45.2530, 19.8620)),
            new HiddenPointDto("/uploads/bedem-kapija.jpg", new LocationDto(45.2520, 19.8640))
        ]);

    private static UpdateLocationChallengeDto UpdateRequest() => new(
        "Tajne Starog grada i Dunavske ulice",
        200,
        new LocationDto(45.2560, 19.8480),
        [
            new HiddenPointDto("/uploads/dunavska-izlog.jpg", new LocationDto(45.2570, 19.8480))
        ]);

    private static LocationDto ToDto(Location location) => new(location.Latitude, location.Longitude);

    private static List<HiddenPointDto> ToDtos(IEnumerable<HiddenPoint> hiddenPoints) =>
        hiddenPoints
            .Select(point => new HiddenPointDto(point.ImageUrl, ToDto(point.Location)))
            .ToList();

    [Fact]
    public async Task Administrator_creates_a_draft_location_challenge_with_its_start_and_all_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateRequest();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == createdId);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
        stored.Name.Should().Be(request.Name);
        stored.Xp.Should().Be(request.Xp);
        ToDto(stored.StartLocation).Should().Be(request.StartLocation);
        ToDtos(stored.HiddenPoints).Should().BeEquivalentTo(request.HiddenPoints, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Location_challenge_with_a_hidden_point_too_far_from_the_start_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateRequest() with
        {
            HiddenPoints = [new HiddenPointDto("/uploads/daleko.jpg", new LocationDto(45.2700, 19.8620))]
        };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", CreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", CreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_draft_location_challenge_and_only_the_new_hidden_points_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = UpdateRequest();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftOldTownSecrets.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.DraftOldTownSecrets.Id);
        stored.Name.Should().Be(request.Name);
        stored.Xp.Should().Be(request.Xp);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
        ToDto(stored.StartLocation).Should().Be(request.StartLocation);
        ToDtos(stored.HiddenPoints).Should().BeEquivalentTo(request.HiddenPoints, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Changing_an_unknown_location_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{unknownId}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Invalid_change_is_rejected_and_the_stored_location_challenge_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.DraftOldTownSecrets;
        var request = UpdateRequest() with
        {
            HiddenPoints = [new HiddenPointDto("/uploads/daleko.jpg", new LocationDto(45.2700, 19.8480))]
        };

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{seeded.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == seeded.Id);
        stored.Name.Should().Be(seeded.Name);
        stored.Xp.Should().Be(seeded.Xp);
        stored.StartLocation.Should().Be(seeded.StartLocation);
        stored.HiddenPoints.Should().Equal(seeded.HiddenPoints);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftOldTownSecrets.Id}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftOldTownSecrets.Id}", UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_a_draft_location_challenge_and_it_is_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == LocationChallengeSeed.DraftQuayByTheBridge.Id);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_location_challenge_and_it_is_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.ArchivedFortressTunnels.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == LocationChallengeSeed.ArchivedFortressTunnels.Id);
    }

    [Fact]
    public async Task Deleting_a_published_location_challenge_is_rejected_and_it_stays_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().Contain(challenge => challenge.Id == LocationChallengeSeed.PublishedStrandBeach.Id);
    }

    [Fact]
    public async Task Deleting_an_unknown_location_challenge_is_rejected_and_nothing_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_location_challenge_with_an_unfinished_attempt_and_both_are_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.ArchivedDanubePark;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == seeded.Id);
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore - 1);
        assertContext.LocationChallengeAttempts.Should().NotContain(attempt => attempt.Id == LocationChallengeAttemptSeed.ExplorersDanubeParkUnfinishedInFirstRound.Id);
    }

    [Fact]
    public async Task Deleting_an_archived_location_challenge_solved_in_an_earlier_round_is_rejected_and_it_stays_stored_with_its_attempts()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.ArchivedCathedralInSecondRoundSolvedInFirstRound;
        var solvedAttempt = LocationChallengeAttemptSeed.ThirdExplorersCathedralSolvedInFirstRound;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        var storedChallenge = assertContext.LocationChallenges.Single(challenge => challenge.Id == seeded.Id);
        storedChallenge.Status.Should().Be(seeded.Status);
        storedChallenge.CurrentRound.Should().Be(seeded.CurrentRound);
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
        var storedAttempt = assertContext.LocationChallengeAttempts.Single(attempt => attempt.Id == solvedAttempt.Id);
        storedAttempt.SolvedAt.Should().Be(solvedAttempt.SolvedAt);
        storedAttempt.EarnedXp.Should().Be(solvedAttempt.EarnedXp);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
