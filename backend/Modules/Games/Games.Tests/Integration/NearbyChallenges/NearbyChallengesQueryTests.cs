using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Application.NearbyChallenges;
using Games.Domain.LocationChallenges;
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
        danubePark.EarnedXp.Should().Be(QuizAttemptSeed.ThirdExplorersDanubeParkSolvedWithTwoOfThree.EarnedXp);
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
    public async Task Published_location_challenges_are_listed_with_the_meters_left_to_the_activation_range_and_started_only_in_the_current_round()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        var locationChallenges = nearby!.Where(challenge => challenge.Type == ChallengeType.LocationChallenge).ToList();
        locationChallenges.Select(challenge => challenge.Id).Should().BeEquivalentTo(
            LocationChallengeSeed.All
                .Where(seeded => seeded.Status == LocationChallengeStatus.Published)
                .Select(seeded => seeded.Id));
        var square = locationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedSynagogueSquare.Id);
        square.MetersToRange.Should().Be(0);
        square.IsStarted.Should().BeFalse();
        var strandBeach = locationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedStrandBeach.Id);
        strandBeach.MetersToRange.Should().Be((int)Math.Ceiling(strandBeach.DistanceInMeters - LocationChallenge.ActivationRangeInMeters));
        strandBeach.IsStarted.Should().BeTrue();
        locationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.Id)
            .IsStarted.Should().BeFalse();
    }

    [Fact]
    public async Task Location_challenge_solved_in_the_current_round_is_listed_as_solved_with_the_earned_xp_and_one_solved_in_an_earlier_round_is_not()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var nearby = await client.GetFromJsonAsync<List<NearbyChallengeDto>>("/api/games/nearby-challenges", JsonOptions);

        var square = nearby!.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedSynagogueSquare.Id);
        square.IsSolved.Should().BeTrue();
        square.EarnedXp.Should().Be(LocationChallengeAttemptSeed.SecondExplorersSynagogueSquareSolvedInCurrentRound.EarnedXp);
        var courtyard = nearby!.Single(challenge => challenge.Id == LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.Id);
        courtyard.IsSolved.Should().BeFalse();
        courtyard.EarnedXp.Should().BeNull();
    }
}
