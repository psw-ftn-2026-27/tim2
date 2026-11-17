using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringCommandTests : BaseIntegrationTest
{
    private static readonly KeyPointDataDto KeyPointData = new(
        "Katolička porta", "Trg iza crkve imena Marijinog.", "/images/porta.jpg", 45.2559, 19.8451, "Na zidu je stari sunčani sat.", true);

    public KeyPointAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_adds_a_key_point_to_their_draft_tour_and_it_is_stored_with_all_its_data_at_the_end_of_the_list()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.DraftOldTownWithThreeKeyPoints.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tourId}/key-points", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == tourId).KeyPoints;
        stored.Should().HaveCount(keyPointCountBefore + 1);
        var added = stored.Single(keyPoint => keyPoint.Name == KeyPointData.Name);
        added.Order.Should().Be(keyPointCountBefore + 1);
        added.Description.Should().Be(KeyPointData.Description);
        added.ImageUrl.Should().Be(KeyPointData.ImageUrl);
        added.Location.Should().Be(new Location(KeyPointData.Latitude, KeyPointData.Longitude));
        added.Secret.Should().Be(KeyPointData.Secret);
        added.IsVisibleBeforePurchase.Should().BeTrue();
    }

    [Fact]
    public async Task Author_cannot_add_a_key_point_to_another_authors_tour_and_its_list_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.SecondExplorersDraftMarketWithTwoKeyPoints.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tourId}/key-points", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Should().HaveCount(keyPointCountBefore);
    }

    [Fact]
    public async Task Key_point_without_a_name_is_rejected_and_the_list_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.DraftOldTownWithThreeKeyPoints.Id;
        var request = KeyPointData with { Name = "  " };
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tourId}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Should().HaveCount(keyPointCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_key_point()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.DraftOldTownWithThreeKeyPoints.Id}/key-points", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.DraftOldTownWithThreeKeyPoints.Id}/key-points", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_changes_a_key_point_of_their_tour_and_the_changed_data_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;
        var keyPoint = tour.KeyPoints[1];

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Single(k => k.Id == keyPoint.Id);
        stored.Name.Should().Be(KeyPointData.Name);
        stored.Description.Should().Be(KeyPointData.Description);
        stored.ImageUrl.Should().Be(KeyPointData.ImageUrl);
        stored.Location.Should().Be(new Location(KeyPointData.Latitude, KeyPointData.Longitude));
        stored.Secret.Should().Be(KeyPointData.Secret);
        stored.IsVisibleBeforePurchase.Should().BeTrue();
        stored.Order.Should().Be(keyPoint.Order);
    }

    [Fact]
    public async Task Author_cannot_change_a_key_point_of_another_authors_tour_and_its_data_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.SecondExplorersDraftMarketWithTwoKeyPoints;
        var keyPoint = tour.KeyPoints[0];

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Single(k => k.Id == keyPoint.Id);
        stored.Name.Should().Be(keyPoint.Name);
        stored.Description.Should().Be(keyPoint.Description);
        stored.Secret.Should().Be(keyPoint.Secret);
    }

    [Fact]
    public async Task Changing_a_key_point_of_a_published_tour_is_rejected_and_its_data_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedBridgesWithTwoKeyPoints;
        var keyPoint = tour.KeyPoints[0];

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Single(k => k.Id == keyPoint.Id);
        stored.Name.Should().Be(keyPoint.Name);
        stored.Description.Should().Be(keyPoint.Description);
        stored.Secret.Should().Be(keyPoint.Secret);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_key_point()
    {
        var client = Factory.CreateClient();
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[0].Id}", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[0].Id}", KeyPointData);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_removes_a_key_point_of_their_tour_and_the_remaining_ones_are_stored_with_shifted_orders()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;
        var removed = tour.KeyPoints.Single(keyPoint => keyPoint.Order == 1);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Count;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{removed.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints;
        stored.Should().HaveCount(keyPointCountBefore - 1);
        stored.Should().NotContain(keyPoint => keyPoint.Id == removed.Id);
        stored.Should().OnlyContain(keyPoint =>
            keyPoint.Order == tour.KeyPoints.Single(seeded => seeded.Id == keyPoint.Id).Order - 1);
    }

    [Fact]
    public async Task Author_cannot_remove_a_key_point_of_another_authors_tour_and_its_list_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.SecondExplorersDraftMarketWithTwoKeyPoints;
        var keyPoint = tour.KeyPoints[0];
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Count;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints;
        stored.Should().HaveCount(keyPointCountBefore);
        stored.Should().Contain(k => k.Id == keyPoint.Id);
    }

    [Fact]
    public async Task Removing_a_missing_key_point_is_rejected_and_the_list_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.DraftOldTownWithThreeKeyPoints.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Count;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tourId}/key-points/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == tourId).KeyPoints.Should().HaveCount(keyPointCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClient();
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[0].Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[0].Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_moves_a_key_point_of_their_tour_and_the_new_order_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;
        var first = tour.KeyPoints.Single(keyPoint => keyPoint.Order == 1);
        var second = tour.KeyPoints.Single(keyPoint => keyPoint.Order == 2);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{second.Id}/move",
            new MoveKeyPointDto(KeyPointMoveDirection.Up), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints;
        stored.Single(keyPoint => keyPoint.Id == second.Id).Order.Should().Be(1);
        stored.Single(keyPoint => keyPoint.Id == first.Id).Order.Should().Be(2);
    }

    [Fact]
    public async Task Author_cannot_move_a_key_point_of_another_authors_tour_and_its_order_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.SecondExplorersDraftMarketWithTwoKeyPoints;
        var second = tour.KeyPoints.Single(keyPoint => keyPoint.Order == 2);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{second.Id}/move",
            new MoveKeyPointDto(KeyPointMoveDirection.Up), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints;
        stored.Should().OnlyContain(keyPoint =>
            keyPoint.Order == tour.KeyPoints.Single(seeded => seeded.Id == keyPoint.Id).Order);
    }

    [Fact]
    public async Task Moving_the_first_key_point_up_is_rejected_and_the_order_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;
        var first = tour.KeyPoints.Single(keyPoint => keyPoint.Order == 1);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{first.Id}/move",
            new MoveKeyPointDto(KeyPointMoveDirection.Up), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints;
        stored.Should().OnlyContain(keyPoint =>
            keyPoint.Order == tour.KeyPoints.Single(seeded => seeded.Id == keyPoint.Id).Order);
    }

    [Fact]
    public async Task Anonymous_user_cannot_move_a_key_point()
    {
        var client = Factory.CreateClient();
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[1].Id}/move",
            new MoveKeyPointDto(KeyPointMoveDirection.Up), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_move_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var tour = TourSeed.DraftOldTownWithThreeKeyPoints;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{tour.Id}/key-points/{tour.KeyPoints[1].Id}/move",
            new MoveKeyPointDto(KeyPointMoveDirection.Up), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_sets_the_calculated_length_of_their_draft_tour_and_it_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.DraftOldTownWithThreeKeyPoints.Id;
        var request = new TourLengthDto(2.75);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tourId}/length", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == tourId).LengthKm.Should().Be(request.LengthKm);
    }

    [Fact]
    public async Task Author_cannot_set_the_length_of_another_authors_tour_and_its_length_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.SecondExplorersDraftMarketWithTwoKeyPoints.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var lengthBefore = arrangeContext.Tours.Single(tour => tour.Id == tourId).LengthKm;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tourId}/length", new TourLengthDto(2.75));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == tourId).LengthKm.Should().Be(lengthBefore);
    }

    [Fact]
    public async Task Setting_the_length_of_a_published_tour_is_rejected_and_its_length_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.PublishedBridgesWithTwoKeyPoints.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var lengthBefore = arrangeContext.Tours.Single(tour => tour.Id == tourId).LengthKm;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{tourId}/length", new TourLengthDto(2.75));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == tourId).LengthKm.Should().Be(lengthBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_set_the_length_of_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.DraftOldTownWithThreeKeyPoints.Id}/length", new TourLengthDto(2.75));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_set_the_length_of_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/tours/{TourSeed.DraftOldTownWithThreeKeyPoints.Id}/length", new TourLengthDto(2.75));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
