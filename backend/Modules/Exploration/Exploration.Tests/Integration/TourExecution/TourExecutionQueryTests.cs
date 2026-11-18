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
}
