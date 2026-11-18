using System.Net;
using FluentAssertions;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public class LocationChallengeSolvingCommandTests : BaseIntegrationTest
{
    public LocationChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Player_in_range_activates_a_published_location_challenge_and_an_attempt_of_the_current_round_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedSynagogueSquare;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{challenge.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore + 1);
        var stored = assertContext.LocationChallengeAttempts.Single(attempt =>
            attempt.ChallengeId == challenge.Id && attempt.PlayerId == WellKnownUsers.ThirdExplorer);
        stored.Round.Should().Be(challenge.CurrentRound);
        stored.SearchRadiusInMeters.Should().BeInRange(challenge.SearchRadiusInMeters(0), challenge.SearchRadiusInMeters(100));
    }

    [Fact]
    public async Task Activating_without_a_saved_position_is_rejected_and_no_attempt_is_created()
    {
        var playerWithoutPosition = Guid.NewGuid();
        var client = Factory.CreateClientFor(playerWithoutPosition, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedSynagogueSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.LocationChallengeAttempts.Should().NotContain(attempt => attempt.PlayerId == playerWithoutPosition);
    }

    [Fact]
    public async Task Activating_from_outside_the_activation_range_is_rejected_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedSynagogueSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.LocationChallengeAttempts.Should().NotContain(attempt =>
            attempt.ChallengeId == LocationChallengeSeed.PublishedSynagogueSquare.Id && attempt.PlayerId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Player_with_an_attempt_from_an_earlier_round_activates_the_republished_challenge_and_both_attempts_are_kept()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var challenge = LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound;
        var earlierAttempt = LocationChallengeAttemptSeed.ThirdExplorersSynagogueCourtyardFromFirstRound;

        var response = await client.PostAsync($"/api/games/location-challenges/{challenge.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts
            .Where(attempt => attempt.ChallengeId == challenge.Id && attempt.PlayerId == WellKnownUsers.ThirdExplorer)
            .ToList();
        stored.Select(attempt => attempt.Round).Should().BeEquivalentTo([earlierAttempt.Round, challenge.CurrentRound]);
        stored.Should().Contain(attempt => attempt.Id == earlierAttempt.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedSynagogueSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedSynagogueSquare.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Player_standing_on_the_last_undiscovered_point_checks_the_spot_and_the_attempt_is_solved_with_the_xp_of_the_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedLimanPark;
        var attempt = LocationChallengeAttemptSeed.ExplorersLimanParkWithoutTheFirstPoint;

        var response = await client.PostAsync($"/api/games/location-challenges/{challenge.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(row => row.Id == attempt.Id);
        stored.DiscoveredPointNumbers.Should().BeEquivalentTo(Enumerable.Range(1, challenge.HiddenPoints.Count));
        stored.SolvedAt.Should().NotBeNull();
        stored.EarnedXp.Should().Be(challenge.Xp);
    }

    [Fact]
    public async Task Checking_without_a_saved_position_is_rejected_and_the_attempt_stays_the_same()
    {
        var attempt = LocationChallengeAttemptSeed.PlayerWithoutPositionsLimanParkInCurrentRound;
        var client = Factory.CreateClientFor(attempt.PlayerId, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedLimanPark.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(row => row.Id == attempt.Id);
        stored.DiscoveredPointNumbers.Should().BeEmpty();
        stored.SolvedAt.Should().BeNull();
    }

    [Fact]
    public async Task Checking_in_a_challenge_that_is_not_published_is_rejected_and_the_attempt_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var attempt = LocationChallengeAttemptSeed.ExplorersFishermansIslandBeforeItWasRetired;

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.RetiredFishermansIsland.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(row => row.Id == attempt.Id);
        stored.DiscoveredPointNumbers.Should().BeEmpty();
        stored.SolvedAt.Should().BeNull();
    }

    [Fact]
    public async Task Checking_away_from_every_hidden_point_succeeds_and_the_attempt_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var attempt = LocationChallengeAttemptSeed.ThirdExplorersStrandBeachInCurrentRound;

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(row => row.Id == attempt.Id);
        stored.DiscoveredPointNumbers.Should().BeEmpty();
        stored.SolvedAt.Should().BeNull();
        stored.EarnedXp.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_hidden_spot()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedLimanPark.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_hidden_spot()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedLimanPark.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
