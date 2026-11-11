using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeManagement;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeManagement;

public class SocialChallengeManagementQueryTests : BaseIntegrationTest
{
    public SocialChallengeManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Overview_lists_the_seeded_challenges_with_the_computed_end_of_their_window_and_their_completion_message()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<SocialChallengeDto>>("/api/games/challenges", JsonOptions);

        challenges!.TotalCount.Should().Be(SocialChallengeSeed.All.Length);
        challenges.Items.Should().BeEquivalentTo(SocialChallengeSeed.All.Select(seeded => new SocialChallengeDto(
            seeded.Id,
            seeded.Name,
            seeded.Window.Start,
            seeded.Window.Duration,
            seeded.Window.End(),
            seeded.RequiredParticipants,
            seeded.RangeInMeters,
            seeded.Xp,
            seeded.CompletionMessage,
            seeded.Status)));
    }
}
