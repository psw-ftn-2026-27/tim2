using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubAuthoring;
using Social.Application.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubAuthoring;

public class ClubAuthoringCommandTests : BaseIntegrationTest
{
    public ClubAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_founds_a_club_and_becomes_its_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajedničke posete muzejima Novog Sada.", ["https://example.com/muzej.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<ClubDto>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore + 1);
        var stored = assertContext.Clubs.Single(club => club.Id == created!.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Name.Should().Be("Ljubitelji muzeja");
        stored.Description.Should().Be("Zajedničke posete muzejima Novog Sada.");
        stored.Images.Should().Equal("https://example.com/muzej.jpg");
    }

    [Fact]
    public async Task Anonymous_user_cannot_found_a_club()
    {
        var client = Factory.CreateClient();
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajedničke posete muzejima Novog Sada.", ["https://example.com/muzej.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_found_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajedničke posete muzejima Novog Sada.", ["https://example.com/muzej.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_changes_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari Fruške gore i okoline", "Vikend i praznične ture po Fruškoj gori.",
            ["https://example.com/vrdnik.jpg", "https://example.com/stazar.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikingClub.Id);
        stored.Name.Should().Be("Planinari Fruške gore i okoline");
        stored.Description.Should().Be("Vikend i praznične ture po Fruškoj gori.");
        stored.Images.Should().Equal("https://example.com/vrdnik.jpg", "https://example.com/stazar.jpg");
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_club()
    {
        var client = Factory.CreateClient();
        var request = new UpdateClubDto("Izmena kluba", "Opis kluba.", ["https://example.com/slika.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateClubDto("Izmena kluba", "Opis kluba.", ["https://example.com/slika.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_deletes_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore - 1);
        assertContext.Clubs.Any(club => club.Id == ClubSeed.HikingClub.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_cannot_delete_another_owners_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
        assertContext.Clubs.Any(club => club.Id == ClubSeed.HikingClub.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
