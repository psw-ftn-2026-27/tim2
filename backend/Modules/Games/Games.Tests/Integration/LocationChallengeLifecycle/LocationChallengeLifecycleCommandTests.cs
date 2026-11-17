using System.Net;
using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeLifecycle;

public class LocationChallengeLifecycleCommandTests : BaseIntegrationTest
{
    public LocationChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_location_challenge_and_it_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.DraftQuayByTheBridge.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Published);
    }

    [Fact]
    public async Task Publishing_an_unknown_location_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{unknownId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Publishing_an_already_published_location_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedStrandBeach.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_location_challenge_and_it_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedStrandBeach.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Retired);
    }

    [Fact]
    public async Task Retiring_an_unknown_location_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{unknownId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Retiring_a_draft_location_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.DraftQuayByTheBridge.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_location_challenge_and_it_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.RetiredFishermansIsland.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.RetiredFishermansIsland.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Archived);
    }

    [Fact]
    public async Task Archiving_an_unknown_location_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{unknownId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Archiving_a_draft_location_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayByTheBridge.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.DraftQuayByTheBridge.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.RetiredFishermansIsland.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.RetiredFishermansIsland.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_repeats_an_archived_location_challenge_and_the_same_challenge_is_stored_as_a_draft_without_a_new_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.ArchivedFortressTunnels.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.ArchivedFortressTunnels.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
    }

    [Fact]
    public async Task Repeating_an_unknown_location_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{unknownId}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Repeating_a_published_location_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedStrandBeach.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedStrandBeach.Id);
        stored.Status.Should().Be(LocationChallengeStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_repeat_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.ArchivedFortressTunnels.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_repeat_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.ArchivedFortressTunnels.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
