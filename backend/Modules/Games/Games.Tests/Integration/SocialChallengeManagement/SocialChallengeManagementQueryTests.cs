using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Locations;
using Games.Application.SocialChallengeManagement;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeManagement;

public class SocialChallengeManagementQueryTests : BaseIntegrationTest
{
    public SocialChallengeManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Social_challenge_details_show_its_window_with_the_computed_end_and_its_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = SocialChallengeSeed.DraftMorningRun;

        var challenge = await client.GetFromJsonAsync<SocialChallengeDto>($"/api/games/challenges/{seeded.Id}", JsonOptions);

        challenge.Should().BeEquivalentTo(
            new SocialChallengeDto(
                seeded.Id,
                seeded.Name,
                seeded.Window.Start,
                seeded.Window.Duration,
                seeded.Window.End(),
                seeded.RequiredParticipants,
                seeded.RangeInMeters,
                seeded.Xp,
                seeded.CompletionMessage,
                seeded.Status,
                new LocationDto(seeded.Location!.Latitude, seeded.Location.Longitude)),
            options => options.ComparingRecordsByMembers());
    }

    [Fact]
    public async Task Social_challenge_without_a_location_has_no_location_in_its_details()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenge = await client.GetFromJsonAsync<SocialChallengeDto>($"/api/games/challenges/{SocialChallengeSeed.DraftSquareMeetup.Id}", JsonOptions);

        challenge!.Location.Should().BeNull();
    }

    [Fact]
    public async Task Details_of_an_unknown_social_challenge_are_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/games/challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
