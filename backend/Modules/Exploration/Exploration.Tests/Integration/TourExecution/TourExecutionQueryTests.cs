using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecution;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Xunit;

namespace Exploration.Tests.Integration.TourExecution;

public class TourExecutionQueryTests : BaseIntegrationTest
{
    public TourExecutionQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Active_tour_shows_its_first_key_point_in_the_authors_order()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var tour = TourSeed.All.Single(tour => tour.Id == activeSession.TourId);
        var firstKeyPoint = tour.KeyPoints.OrderBy(keyPoint => keyPoint.Order).First();
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var activeTour = await client.GetFromJsonAsync<ActiveTourDto>($"/api/exploration/tours/{tour.Id}/active", JsonOptions);

        activeTour!.TourName.Should().Be(tour.Name);
        activeTour.NextKeyPoint.Name.Should().Be(firstKeyPoint.Name);
        activeTour.NextKeyPoint.Latitude.Should().Be(firstKeyPoint.Location.Latitude);
        activeTour.NextKeyPoint.Longitude.Should().Be(firstKeyPoint.Location.Longitude);
    }

    [Fact]
    public async Task A_tour_that_is_not_active_for_the_tourist_returns_no_content()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishedCheapGallery.Id}/active");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Another_tourists_active_tour_is_not_shown()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(TourPurchaseSeed.Buyer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{activeSession.TourId}/active");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
