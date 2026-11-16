using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.ChallengeOverview;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.ChallengeOverview;

public class ChallengeOverviewQueryTests : BaseIntegrationTest
{
    public ChallengeOverviewQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Overview_filtered_to_social_challenges_lists_the_seeded_social_challenges_with_the_computed_end_of_their_window()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<ChallengeDto>>("/api/games/challenges?type=SocialChallenge", JsonOptions);

        challenges!.TotalCount.Should().Be(SocialChallengeSeed.All.Length);
        challenges.Items.Should().BeEquivalentTo(SocialChallengeSeed.All.Select(seeded => new ChallengeDto(
            seeded.Id,
            ChallengeType.SocialChallenge,
            seeded.Name,
            seeded.Xp,
            seeded.Status.ToString(),
            seeded.Window.Start,
            seeded.Window.Duration,
            seeded.Window.End(),
            seeded.RequiredParticipants,
            seeded.RangeInMeters,
            seeded.CompletionMessage)));
    }

    [Fact]
    public async Task Overview_filtered_to_quizzes_lists_only_the_seeded_quizzes_with_empty_social_challenge_columns()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<ChallengeDto>>("/api/games/challenges?type=Quiz", JsonOptions);

        challenges!.TotalCount.Should().Be(QuizSeed.All.Length);
        challenges.Items.Should().BeEquivalentTo(QuizSeed.All.Select(seeded => new ChallengeDto(
            seeded.Id,
            ChallengeType.Quiz,
            seeded.Name,
            seeded.Xp,
            seeded.Status.ToString(),
            null,
            null,
            null,
            null,
            null,
            null)));
    }

    [Fact]
    public async Task Overview_filtered_to_location_challenges_lists_only_the_seeded_location_challenges_with_empty_social_challenge_columns()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<ChallengeDto>>("/api/games/challenges?type=LocationChallenge", JsonOptions);

        challenges!.TotalCount.Should().Be(LocationChallengeSeed.All.Length);
        challenges.Items.Should().BeEquivalentTo(LocationChallengeSeed.All.Select(seeded => new ChallengeDto(
            seeded.Id,
            ChallengeType.LocationChallenge,
            seeded.Name,
            seeded.Xp,
            seeded.Status.ToString(),
            null,
            null,
            null,
            null,
            null,
            null)));
    }

    [Fact]
    public async Task Overview_without_a_filter_lists_social_challenges_quizzes_and_location_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<ChallengeDto>>("/api/games/challenges", JsonOptions);

        challenges!.TotalCount.Should().Be(SocialChallengeSeed.All.Length + QuizSeed.All.Length + LocationChallengeSeed.All.Length);
        challenges.Items.Select(challenge => challenge.Id).Should().BeEquivalentTo(
            SocialChallengeSeed.All.Select(seeded => seeded.Id)
                .Concat(QuizSeed.All.Select(seeded => seeded.Id))
                .Concat(LocationChallengeSeed.All.Select(seeded => seeded.Id)));
    }

    [Fact]
    public async Task Second_page_of_the_overview_continues_the_list_sorted_by_name_with_challenges_of_all_three_kinds()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<ChallengeDto>>("/api/games/challenges?page=2&pageSize=3", JsonOptions);

        challenges!.Items.Select(challenge => challenge.Id).Should().Equal(
            SocialChallengeSeed.ArchivedMarketVisit.Id,
            LocationChallengeSeed.DraftQuayByTheBridge.Id,
            QuizSeed.DraftMuseum.Id);
    }
}
