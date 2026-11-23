using System.Net;
using Exploration.Domain.TourSessions;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecution;

public class TourExecutionCommandTests : BaseIntegrationTest
{
    public TourExecutionCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_starts_a_purchased_tour_and_an_active_session_begins_at_their_position()
    {
        var touristId = TourPurchaseSeed.Buyer;
        var tourId = TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint.Id;
        var position = UserPositionSeed.ThirdExplorersPositionAtSynagogue.Coordinates;
        var client = Factory.CreateClientFor(touristId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var countBefore = arrangeContext.TourSessions.Count(session => session.TouristId == touristId);

        var response = await client.PostAsync($"/api/exploration/tours/{tourId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var sessions = assertContext.TourSessions.Where(session => session.TouristId == touristId).ToList();
        sessions.Should().HaveCount(countBefore + 1);
        var started = sessions.Single(session => session.TourId == tourId);
        started.Status.Should().Be(TourSessionStatus.Active);
        started.StartPosition.Latitude.Should().Be(position.Latitude);
        started.StartPosition.Longitude.Should().Be(position.Longitude);
        started.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Starting_the_tour_that_is_already_active_resumes_its_session_without_creating_a_new_one()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var countBefore = arrangeContext.TourSessions.Count(session => session.TouristId == activeSession.TouristId);

        var response = await client.PostAsync($"/api/exploration/tours/{activeSession.TourId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var sessions = assertContext.TourSessions.Where(session => session.TouristId == activeSession.TouristId).ToList();
        sessions.Should().HaveCount(countBefore);
        sessions.Single(session => session.Status == TourSessionStatus.Active).Id.Should().Be(activeSession.Id);
    }

    // The spec assumes that a bought tour stays the buyer's even after its author archives it.
    [Fact]
    public async Task Tourist_starts_a_purchased_tour_that_the_author_has_since_archived()
    {
        var touristId = TourPurchaseSeed.Buyer;
        var tourId = TourSeed.ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Id;
        var client = Factory.CreateClientFor(touristId, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{tourId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourSessions
            .Should().ContainSingle(session => session.TouristId == touristId && session.TourId == tourId && session.Status == TourSessionStatus.Active);
    }

    [Fact]
    public async Task Starting_another_tour_while_one_is_active_is_rejected_and_no_session_is_created()
    {
        var touristId = TourSessionSeed.ExplorersActiveFortressUnderground.TouristId;
        var client = Factory.CreateClientFor(touristId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var countBefore = arrangeContext.TourSessions.Count(session => session.TouristId == touristId);

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedCheapGallery.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourSessions.Count(session => session.TouristId == touristId).Should().Be(countBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_leaves_the_active_tour_and_the_session_is_abandoned_with_its_time()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{activeSession.TourId}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourSessions.Single(session => session.Id == activeSession.Id);
        stored.Status.Should().Be(TourSessionStatus.Abandoned);
        stored.AbandonedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Leaving_a_tour_that_is_not_active_for_the_tourist_is_rejected()
    {
        var activeSession = TourSessionSeed.ExplorersActiveFortressUnderground;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedCheapGallery.Id}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourSessions
            .Where(session => session.TouristId == activeSession.TouristId)
            .Should().NotContain(session => session.Status == TourSessionStatus.Abandoned);
    }

    [Fact]
    public async Task Anonymous_user_cannot_leave_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSessionSeed.ExplorersActiveFortressUnderground.TourId}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_leave_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSessionSeed.ExplorersActiveFortressUnderground.TourId}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_standing_at_the_next_key_point_checks_arrival_and_it_is_completed_with_the_activity_recorded()
    {
        var activeSession = TourSessionSeed.SecondExplorersActiveKnezMihailovaWithFirstKeyPointReached;
        var tour = TourSeed.ArchivedKnezMihailovaWithSecondKeyPointAtSecondExplorersPosition;
        var client = Factory.CreateClientFor(activeSession.TouristId, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/check-arrival", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourSessions.Single(session => session.Id == activeSession.Id);
        stored.CompletedKeyPoints.Select(completion => completion.KeyPointId)
            .Should().Equal(tour.KeyPoints[0].Id, tour.KeyPoints[1].Id);
        stored.CompletedKeyPoints[1].CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        stored.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        stored.Status.Should().Be(TourSessionStatus.Active);
    }

    [Fact]
    public async Task Checking_arrival_without_an_active_session_is_rejected_and_nothing_is_recorded()
    {
        var completedSession = TourSessionSeed.BuyersCompletedKalemegdan;
        var client = Factory.CreateClientFor(completedSession.TouristId, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{completedSession.TourId}/check-arrival", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourSessions.Single(session => session.Id == completedSession.Id);
        stored.LastActivityAt.Should().BeNull();
        stored.CompletedKeyPoints.Should().HaveCount(completedSession.CompletedKeyPoints.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_arrival()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSessionSeed.ExplorersActiveFortressUnderground.TourId}/check-arrival", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_arrival()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSessionSeed.ExplorersActiveFortressUnderground.TourId}/check-arrival", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
