using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubReading;

public class ClubReadingQueryTests : BaseIntegrationTest
{
    public ClubReadingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Clubs_of_all_owners_are_listed_with_name_description_and_images()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs", JsonOptions);

        clubs!.TotalCount.Should().Be(ClubSeed.All.Length);
        clubs.Items.Should().Contain(club => club.Id == ClubSeed.HikingClub.Id);
        var otherOwnersClub = clubs.Items.Single(club => club.Id == ClubSeed.SecondExplorersCyclingClub.Id);
        otherOwnersClub.Name.Should().Be(ClubSeed.SecondExplorersCyclingClub.Name);
        otherOwnersClub.Description.Should().Be(ClubSeed.SecondExplorersCyclingClub.Description);
        otherOwnersClub.Images.Should().Equal(ClubSeed.SecondExplorersCyclingClub.Images);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_clubs()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/clubs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
