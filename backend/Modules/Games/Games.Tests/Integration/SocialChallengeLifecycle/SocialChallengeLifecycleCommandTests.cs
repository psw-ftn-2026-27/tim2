using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeLifecycle;

public class SocialChallengeLifecycleCommandTests : BaseIntegrationTest
{
    public SocialChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_challenge_and_it_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftNightGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftNightGathering.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Publishing_an_unknown_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsync($"/api/games/challenges/{unknownId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Publishing_an_already_published_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedParkPicnic.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedParkPicnic.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftNightGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftNightGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_challenge_and_it_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedParkPicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedParkPicnic.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Fact]
    public async Task Retiring_an_unknown_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsync($"/api/games/challenges/{unknownId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Retiring_a_draft_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftNightGathering.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftNightGathering.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedParkPicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedParkPicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_challenge_and_it_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredBridgeWalk.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.RetiredBridgeWalk.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Fact]
    public async Task Archiving_an_unknown_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsync($"/api/games/challenges/{unknownId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Archiving_a_draft_challenge_is_rejected_and_its_stored_state_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftNightGathering.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftNightGathering.Id);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredBridgeWalk.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredBridgeWalk.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_repeats_a_retired_challenge_and_a_new_draft_with_the_same_data_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var original = SocialChallengeSeed.RetiredBridgeWalk;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsync($"/api/games/challenges/{original.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var copyId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        copyId.Should().NotBe(original.Id);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == copyId);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
        stored.Name.Should().Be(original.Name);
        stored.Window.Should().Be(original.Window);
        stored.RequiredParticipants.Should().Be(original.RequiredParticipants);
        stored.RangeInMeters.Should().Be(original.RangeInMeters);
        stored.Xp.Should().Be(original.Xp);
        stored.CompletionMessage.Should().Be(original.CompletionMessage);
        assertContext.SocialChallenges.Single(challenge => challenge.Id == original.Id).Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Fact]
    public async Task Repeating_an_unknown_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsync($"/api/games/challenges/{Guid.NewGuid()}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Repeating_a_draft_challenge_is_rejected_and_no_new_challenge_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftNightGathering.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_repeat_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredBridgeWalk.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_repeat_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredBridgeWalk.Id}/repeat", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
