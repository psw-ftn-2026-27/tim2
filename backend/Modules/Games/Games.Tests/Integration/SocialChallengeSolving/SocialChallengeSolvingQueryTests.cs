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
