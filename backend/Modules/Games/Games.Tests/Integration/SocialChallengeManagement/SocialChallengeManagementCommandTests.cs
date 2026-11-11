using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeManagement;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeManagement;

public class SocialChallengeManagementCommandTests : BaseIntegrationTest
{
    public SocialChallengeManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_draft_challenge_with_the_entered_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Večernja šetnja", new TimeOnly(19, 0), TimeSpan.FromMinutes(90), 4, 150, 30, "Prošetali ste zajedno.");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == createdId);
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
        stored.Name.Should().Be("Večernja šetnja");
        stored.Window.Start.Should().Be(new TimeOnly(19, 0));
        stored.Window.Duration.Should().Be(TimeSpan.FromMinutes(90));
        stored.RequiredParticipants.Should().Be(4);
        stored.RangeInMeters.Should().Be(150);
        stored.Xp.Should().Be(30);
        stored.CompletionMessage.Should().Be("Prošetali ste zajedno.");
    }

    [Fact]
    public async Task Challenge_with_invalid_data_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Večernja šetnja", new TimeOnly(19, 0), TimeSpan.FromMinutes(90), 1, 150, 30, "Prošetali ste zajedno.");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_challenge()
    {
        var client = Factory.CreateClient();
        var request = new CreateSocialChallengeDto("Večernja šetnja", new TimeOnly(19, 0), TimeSpan.FromMinutes(90), 4, 150, 30, "Prošetali ste zajedno.");

        var response = await client.PostAsJsonAsync("/api/games/challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateSocialChallengeDto("Večernja šetnja", new TimeOnly(19, 0), TimeSpan.FromMinutes(90), 4, 150, 30, "Prošetali ste zajedno.");

        var response = await client.PostAsJsonAsync("/api/games/challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_draft_challenge_and_the_new_values_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Jutarnje trčanje uz Dunav", new TimeOnly(6, 30), TimeSpan.FromMinutes(75), 8, 300, 70, "Svaka čast, ranoranioci!");

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftMorningRun.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftMorningRun.Id);
        stored.Name.Should().Be("Jutarnje trčanje uz Dunav");
        stored.Window.Start.Should().Be(new TimeOnly(6, 30));
        stored.Window.Duration.Should().Be(TimeSpan.FromMinutes(75));
        stored.RequiredParticipants.Should().Be(8);
        stored.RangeInMeters.Should().Be(300);
        stored.Xp.Should().Be(70);
        stored.CompletionMessage.Should().Be("Svaka čast, ranoranioci!");
        stored.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public async Task Changing_an_unknown_challenge_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownId = Guid.NewGuid();
        var request = new UpdateSocialChallengeDto("Jutarnje trčanje uz Dunav", new TimeOnly(6, 30), TimeSpan.FromMinutes(75), 8, 300, 70, "Svaka čast, ranoranioci!");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{unknownId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == unknownId);
    }

    [Fact]
    public async Task Changing_a_challenge_with_invalid_data_leaves_the_stored_challenge_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Jutarnje trčanje uz Dunav", new TimeOnly(6, 30), TimeSpan.FromMinutes(75), 8, 300, 0, "Svaka čast, ranoranioci!");

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftMorningRun.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftMorningRun.Id);
        stored.Name.Should().Be(SocialChallengeSeed.DraftMorningRun.Name);
        stored.Window.Should().Be(SocialChallengeSeed.DraftMorningRun.Window);
        stored.RequiredParticipants.Should().Be(SocialChallengeSeed.DraftMorningRun.RequiredParticipants);
        stored.RangeInMeters.Should().Be(SocialChallengeSeed.DraftMorningRun.RangeInMeters);
        stored.Xp.Should().Be(SocialChallengeSeed.DraftMorningRun.Xp);
        stored.CompletionMessage.Should().Be(SocialChallengeSeed.DraftMorningRun.CompletionMessage);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_challenge()
    {
        var client = Factory.CreateClient();
        var request = new UpdateSocialChallengeDto("Jutarnje trčanje uz Dunav", new TimeOnly(6, 30), TimeSpan.FromMinutes(75), 8, 300, 70, "Svaka čast, ranoranioci!");

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftMorningRun.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateSocialChallengeDto("Jutarnje trčanje uz Dunav", new TimeOnly(6, 30), TimeSpan.FromMinutes(75), 8, 300, 70, "Svaka čast, ranoranioci!");

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftMorningRun.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_a_draft_challenge_and_it_is_gone()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSquareMeetup.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == SocialChallengeSeed.DraftSquareMeetup.Id);
    }

    [Fact]
    public async Task Deleting_an_unknown_challenge_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Deleting_a_published_challenge_is_rejected_and_it_stays_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedParkPicnic.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().Contain(challenge => challenge.Id == SocialChallengeSeed.PublishedParkPicnic.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSquareMeetup.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSquareMeetup.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
