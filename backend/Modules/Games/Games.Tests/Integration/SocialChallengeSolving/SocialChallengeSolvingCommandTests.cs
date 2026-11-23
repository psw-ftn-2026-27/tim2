using System.Net;
using FluentAssertions;
using Games.Domain.SocialChallengeRuns;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeSolving;

public class SocialChallengeSolvingCommandTests : BaseIntegrationTest
{
    public SocialChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Player_in_range_activates_a_social_challenge_and_is_written_into_its_active_run()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedSquareGatheringOpenNow;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.SocialChallengeRuns.Count();

        var response = await client.PostAsync($"/api/games/challenges/{challenge.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns.Count().Should().Be(runCountBefore + 1);
        var stored = assertContext.SocialChallengeRuns.Single(run => run.ChallengeId == challenge.Id);
        stored.Status.Should().Be(SocialChallengeRunStatus.Active);
        stored.Participants.Select(participant => participant.PlayerId).Should().Equal(WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Activating_without_a_saved_position_is_rejected_and_no_run_is_created()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var challenge = SocialChallengeSeed.PublishedSquareGatheringOpenNow;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.SocialChallengeRuns.Count();

        var response = await client.PostAsync($"/api/games/challenges/{challenge.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns.Count().Should().Be(runCountBefore);
        assertContext.SocialChallengeRuns.Should().NotContain(run => run.ChallengeId == challenge.Id);
    }

    [Fact]
    public async Task Activating_from_outside_the_range_is_rejected_and_no_run_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedSquareGatheringOpenNow;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.SocialChallengeRuns.Count();

        var response = await client.PostAsync($"/api/games/challenges/{challenge.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns.Count().Should().Be(runCountBefore);
        assertContext.SocialChallengeRuns.Should().NotContain(run => run.ChallengeId == challenge.Id);
    }

    [Fact]
    public async Task Activating_while_an_ended_run_is_still_active_closes_the_old_run_and_stores_a_new_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var endedRun = SocialChallengeRunSeed.CathedralMeetupEndedYesterdayButStillActive;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.SocialChallengeRuns.Count();

        var response = await client.PostAsync($"/api/games/challenges/{endedRun.ChallengeId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns.Count().Should().Be(runCountBefore + 1);
        assertContext.SocialChallengeRuns.Single(run => run.Id == endedRun.Id).Status.Should().Be(SocialChallengeRunStatus.Expired);
        var newRun = assertContext.SocialChallengeRuns.Single(run => run.ChallengeId == endedRun.ChallengeId && run.Id != endedRun.Id);
        newRun.Status.Should().Be(SocialChallengeRunStatus.Active);
        newRun.Participants.Select(participant => participant.PlayerId).Should().Equal(WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedSquareGatheringOpenNow.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedSquareGatheringOpenNow.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Check_that_completes_the_number_of_players_solves_the_run_and_the_solution_is_written()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var seeded = SocialChallengeRunSeed.PromenadeWalkWithEnoughPlayersAwaitingCheck;

        var response = await client.PostAsync($"/api/games/challenges/{seeded.ChallengeId}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallengeRuns.Single(run => run.Id == seeded.Id);
        stored.Status.Should().Be(SocialChallengeRunStatus.Solved);
        stored.SolvedAt.Should().NotBeNull();
        stored.EarnedXp.Should().Be(SocialChallengeSeed.PublishedPromenadeWalkOpenNow.Xp);
        stored.Participants.Select(participant => participant.PlayerId).Should().BeEquivalentTo([WellKnownUsers.ThirdExplorer, WellKnownUsers.Explorer]);
    }

    [Fact]
    public async Task Check_by_a_player_who_solved_the_challenge_leaves_a_later_run_of_other_players_untouched()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var laterRun = SocialChallengeRunSeed.RiverbankActiveRunOfAnotherPlayer;

        var response = await client.PostAsync($"/api/games/challenges/{laterRun.ChallengeId}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallengeRuns.Single(run => run.Id == laterRun.Id);
        stored.Status.Should().Be(SocialChallengeRunStatus.Active);
        stored.Participants.Select(participant => participant.PlayerId).Should().Equal(WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Check_by_a_player_who_is_not_in_the_run_is_rejected_and_nothing_is_written()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = SocialChallengeRunSeed.PromenadeWalkWithEnoughPlayersAwaitingCheck;

        var response = await client.PostAsync($"/api/games/challenges/{seeded.ChallengeId}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallengeRuns.Single(run => run.Id == seeded.Id);
        stored.Status.Should().Be(SocialChallengeRunStatus.Active);
        stored.SolvedAt.Should().BeNull();
        stored.Participants.Select(participant => participant.PlayerId).Should().BeEquivalentTo(seeded.Participants.Select(participant => participant.PlayerId));
    }

    [Fact]
    public async Task Check_of_a_challenge_without_a_run_is_rejected_and_no_run_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedSquareGatheringOpenNow;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.SocialChallengeRuns.Count();

        var response = await client.PostAsync($"/api/games/challenges/{challenge.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallengeRuns.Count().Should().Be(runCountBefore);
        assertContext.SocialChallengeRuns.Should().NotContain(run => run.ChallengeId == challenge.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_social_challenge_run()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedPromenadeWalkOpenNow.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_social_challenge_run()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedPromenadeWalkOpenNow.Id}/checks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
