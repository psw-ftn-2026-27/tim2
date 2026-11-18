using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.NearbyChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.NearbyChallenges;

public class NearbyChallengesQueryTests : BaseIntegrationTest
{
    public NearbyChallengesQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Nearby_challenges_are_listed_from_the_closest_to_the_farthest()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        nearby!.Select(challenge => challenge.DistanceInMeters).Should().BeInAscendingOrder();
        nearby!.Select(challenge => challenge.Id).Should().ContainInOrder(
            QuizSeed.PublishedSynagogue.Id,
            QuizSeed.PublishedDanubePark.Id);
    }

    [Fact]
    public async Task Quiz_whose_range_the_player_stands_in_has_no_meters_left_to_walk()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        var synagogue = nearby!.Single(challenge => challenge.Id == QuizSeed.PublishedSynagogue.Id);
        synagogue.MetersToRange.Should().Be(0);
        nearby!.Single(challenge => challenge.Id == QuizSeed.PublishedDanubePark.Id).MetersToRange.Should().BePositive();
    }

    [Fact]
    public async Task Quiz_the_player_solved_is_listed_as_solved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        var danubePark = nearby!.Single(challenge => challenge.Id == QuizSeed.PublishedDanubePark.Id);
        danubePark.IsStarted.Should().BeTrue();
        danubePark.IsSolved.Should().BeTrue();
        nearby!.Single(challenge => challenge.Id == QuizSeed.PublishedSynagogue.Id).IsSolved.Should().BeFalse();
    }

    [Fact]
    public async Task Player_without_a_saved_position_gets_an_empty_list()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        nearby.Should().BeEmpty();
    }

    [Fact]
    public async Task Draft_quiz_and_quiz_without_a_location_are_not_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        nearby!.Select(challenge => challenge.Id).Should()
            .Contain(QuizSeed.PublishedSynagogue.Id)
            .And.NotContain(QuizSeed.DraftCityHall.Id)
            .And.NotContain(QuizSeed.DraftFortressArchitecture.Id);
    }

    [Fact]
    public async Task Social_challenges_show_their_window_run_and_solved_states_and_drafts_and_challenges_without_a_location_are_not_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        var closed = nearby!.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedEveningConcertClosedNow.Id);
        closed.IsWindowOpen.Should().BeFalse();
        closed.WindowStart.Should().Be(SocialChallengeSeed.PublishedEveningConcertClosedNow.Window.Start);
        closed.RequiredParticipants.Should().Be(SocialChallengeSeed.PublishedEveningConcertClosedNow.RequiredParticipants);
        var inRun = nearby!.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedPromenadeWalkOpenNow.Id);
        inRun.IsWindowOpen.Should().BeTrue();
        inRun.IsInActiveRun.Should().BeTrue();
        inRun.IsSolved.Should().BeFalse();
        var solved = nearby!.Single(challenge => challenge.Id == SocialChallengeSeed.RepublishedRiverbankWithMoreXp.Id);
        solved.IsSolved.Should().BeTrue();
        solved.EarnedXp.Should().Be(SocialChallengeRunSeed.RiverbankSolvedWithFortyXp.EarnedXp);
        nearby!.Select(challenge => challenge.Id).Should()
            .NotContain(SocialChallengeSeed.DraftMorningRun.Id)
            .And.NotContain(SocialChallengeSeed.DraftSquareMeetup.Id);
    }
}
