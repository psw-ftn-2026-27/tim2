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
}
