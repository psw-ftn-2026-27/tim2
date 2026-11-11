using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"]);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore + 1);
        var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.TransportTimes.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_creates_a_tour_with_a_recommended_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"],
            new RecommendedGroupSizeDto(2, 8));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        created!.RecommendedGroupSize.Should().Be(new RecommendedGroupSizeDto(2, 8));
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
        stored.RecommendedGroupSize.Should().Be(new RecommendedGroupSize(2, 8));
    }

    [Fact]
    public async Task Author_creates_a_tour_with_a_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"], Price: 700);

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == created!.Id);
        stored.Price.Should().Be(700);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_tour_without_a_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("   ", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"]);

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_adds_a_transport_time_to_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TransportTimeDto(TransportMode.Walking, 120);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.TransportTimes.Should().ContainSingle(time => time.Transport == TransportMode.Walking && time.Minutes == 120);
    }

    [Fact]
    public async Task Author_changes_the_recommended_group_size_of_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RecommendedGroupSizeDto(3, 6);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/recommended-group-size", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.RecommendedGroupSize.Should().Be(new RecommendedGroupSize(3, 6));
    }

    [Fact]
    public async Task Invalid_recommended_group_size_leaves_the_stored_one_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RecommendedGroupSizeDto(6, 3);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/recommended-group-size", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.RecommendedGroupSize.Should().Be(TourSeed.PublishableRiverside.RecommendedGroupSize);
    }

    [Fact]
    public async Task Explorer_cannot_change_the_recommended_group_size_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RecommendedGroupSizeDto(3, 6);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.SecondExplorersTrail.Id}/recommended-group-size", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
        stored.RecommendedGroupSize.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_the_recommended_group_size()
    {
        var client = Factory.CreateClient();
        var request = new RecommendedGroupSizeDto(3, 6);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/recommended-group-size", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_the_recommended_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new RecommendedGroupSizeDto(3, 6);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/recommended-group-size", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_changes_the_price_of_their_own_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPriceDto(2500);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/price", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Price.Should().Be(2500);
    }

    [Fact]
    public async Task Negative_price_leaves_the_stored_price_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPriceDto(-100);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/price", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Price.Should().Be(TourSeed.PublishableRiverside.Price);
    }

    [Fact]
    public async Task Explorer_cannot_change_the_price_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPriceDto(2500);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.SecondExplorersTrail.Id}/price", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
        stored.Price.Should().Be(TourSeed.SecondExplorersTrail.Price);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_the_price_of_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new TourPriceDto(2500);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/price", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_the_price_of_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new TourPriceDto(2500);

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/price", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tour_is_published_when_all_rules_are_met()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }
}
