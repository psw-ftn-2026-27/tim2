using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecution;
using Exploration.Domain.TourSessions;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Xunit;

namespace Exploration.Tests.Integration.TourExecution;

public class TourExecutionQueryTests : BaseIntegrationTest
{
    public TourExecutionQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Progress_of_a_freshly_started_tour_shows_its_first_key_point_in_the_authors_order()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var tour = TourSeed.All.Single(tour => tour.Id == activeSession.TourId);
        var firstKeyPoint = tour.KeyPoints.OrderBy(keyPoint => keyPoint.Order).First();
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var progress = await client.GetFromJsonAsync<TourProgressDto>($"/api/exploration/tours/{tour.Id}/progress", JsonOptions);

        progress!.TourName.Should().Be(tour.Name);
        progress.Status.Should().Be(TourSessionStatus.Active);
        progress.NextKeyPoint!.Name.Should().Be(firstKeyPoint.Name);
        progress.NextKeyPoint.Latitude.Should().Be(firstKeyPoint.Location.Latitude);
        progress.NextKeyPoint.Longitude.Should().Be(firstKeyPoint.Location.Longitude);
        progress.CompletedKeyPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Tour_progress_reveals_the_secrets_of_completed_key_points_only()
    {
        var activeSession = TourSessionSeed.SecondExplorersActiveKnezMihailovaWithFirstKeyPointReached;
        var tour = TourSeed.ArchivedKnezMihailovaWithSecondKeyPointAtSecondExplorersPosition;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var progress = await client.GetFromJsonAsync<TourProgressDto>($"/api/exploration/tours/{tour.Id}/progress", JsonOptions);

        progress!.CompletedKeyPoints.Should().Equal(new CompletedKeyPointDto(tour.KeyPoints[0].Name, tour.KeyPoints[0].Secret));
        progress.NextKeyPoint!.Name.Should().Be(tour.KeyPoints[1].Name);
    }

    [Fact]
    public async Task Progress_of_a_completed_tour_shows_the_completed_session_without_a_next_key_point()
    {
        var completedSession = TourSessionSeed.BuyersCompletedKalemegdan;
        var tour = TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade;
        var client = Factory.CreateClientFor(completedSession.TouristId, "explorer");

        var progress = await client.GetFromJsonAsync<TourProgressDto>($"/api/exploration/tours/{tour.Id}/progress", JsonOptions);

        progress!.Status.Should().Be(TourSessionStatus.Completed);
        progress.NextKeyPoint.Should().BeNull();
        progress.CompletedKeyPoints.Should().Equal(tour.KeyPoints
            .OrderBy(keyPoint => keyPoint.Order)
            .Select(keyPoint => new CompletedKeyPointDto(keyPoint.Name, keyPoint.Secret)));
    }

    [Fact]
    public async Task A_tour_the_tourist_has_not_started_returns_no_content()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishedCheapGallery.Id}/progress");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Another_tourists_tour_progress_is_not_shown()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(TourPurchaseSeed.Buyer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{activeSession.TourId}/progress");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
