using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringQueryTests : BaseIntegrationTest
{
    public KeyPointAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_gets_the_name_and_status_of_their_tour_and_all_its_key_points_in_order_with_secrets()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;
        var expectedKeyPoints = tour.KeyPoints.OrderBy(keyPoint => keyPoint.Order).ToList();

        var keyPoints = await client.GetFromJsonAsync<TourKeyPointsDto>(
            $"/api/exploration/tours/{tour.Id}/key-points", JsonOptions);

        keyPoints!.Name.Should().Be(tour.Name);
        keyPoints.Status.Should().Be(tour.Status);
        keyPoints.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(expectedKeyPoints.Select(keyPoint => keyPoint.Id));
        keyPoints.KeyPoints.Select(keyPoint => keyPoint.Order).Should().Equal(expectedKeyPoints.Select(keyPoint => keyPoint.Order));
        keyPoints.KeyPoints.Select(keyPoint => keyPoint.Secret).Should().Equal(expectedKeyPoints.Select(keyPoint => keyPoint.Secret));
    }

    [Fact]
    public async Task Author_does_not_get_the_key_points_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(
            $"/api/exploration/tours/{TourSeed.SecondExplorersDraftMarketWithTwoKeyPoints.Id}/key-points");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
