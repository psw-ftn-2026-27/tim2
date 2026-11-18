using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeSolving;

public class SocialChallengeSolvingQueryTests : BaseIntegrationTest
{
    public SocialChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Solved_run_shows_the_recorded_xp_even_after_the_challenge_xp_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var challenge = SocialChallengeSeed.RepublishedRiverbankWithMoreXp;
        var seeded = SocialChallengeRunSeed.RiverbankSolvedWithFortyXp;

        var progress = await client.GetFromJsonAsync<SocialChallengeProgressDto>(
            $"/api/games/challenges/{challenge.Id}/progress", JsonOptions);

        progress!.State.Should().Be(SocialChallengeProgressState.Solved);
        progress.EarnedXp.Should().Be(seeded.EarnedXp).And.NotBe(challenge.Xp);
        progress.SolvedAt.Should().Be(seeded.SolvedAt);
        progress.CompletionMessage.Should().Be(challenge.CompletionMessage);
    }

    [Fact]
    public async Task Player_in_an_active_run_sees_the_run_with_its_participants_and_no_result_yet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = SocialChallengeRunSeed.PromenadeWalkWithEnoughPlayersAwaitingCheck;

        var progress = await client.GetFromJsonAsync<SocialChallengeProgressDto>(
            $"/api/games/challenges/{seeded.ChallengeId}/progress", JsonOptions);

        progress!.State.Should().Be(SocialChallengeProgressState.InRun);
        progress.ParticipantCount.Should().Be(seeded.Participants.Count);
        progress.LastCheckedAt.Should().NotBeNull();
        progress.EarnedXp.Should().BeNull();
        progress.CompletionMessage.Should().BeNull();
    }

    [Fact]
    public async Task Player_outside_the_active_run_of_a_challenge_is_shown_as_not_in_a_run()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = SocialChallengeRunSeed.PromenadeWalkWithEnoughPlayersAwaitingCheck;

        var progress = await client.GetFromJsonAsync<SocialChallengeProgressDto>(
            $"/api/games/challenges/{seeded.ChallengeId}/progress", JsonOptions);

        progress!.State.Should().Be(SocialChallengeProgressState.NotInRun);
        progress.LastCheckedAt.Should().BeNull();
        progress.EarnedXp.Should().BeNull();
    }

    [Fact]
    public async Task Run_past_its_end_shows_as_expired_before_any_check_writes_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var seeded = SocialChallengeRunSeed.CathedralMeetupEndedYesterdayButStillActive;

        var progress = await client.GetFromJsonAsync<SocialChallengeProgressDto>(
            $"/api/games/challenges/{seeded.ChallengeId}/progress", JsonOptions);

        progress!.State.Should().Be(SocialChallengeProgressState.Expired);
        progress.EarnedXp.Should().BeNull();
    }
}
