using Exploration.Domain.Locations;
using Exploration.Domain.Tours;
using Exploration.Domain.TourSessions;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourSessions;

public class KeyPointArrivalTests
{
    private static readonly Location FirstKeyPointLocation = new(45.2500, 19.8500);
    private static readonly Location SecondKeyPointLocation = new(45.2600, 19.8600);

    private readonly KeyPointArrival _keyPointArrival = new();

    private static Tour CreateTourWithTwoKeyPoints()
    {
        var tour = new Tour(WellKnownUsers.SecondExplorer, "Tura", "Kratak opis ture.", TourDifficulty.Easy, ["grad"]);
        tour.AddKeyPoint("Prva tačka", "Opis prve tačke.", "/images/prva.jpg", FirstKeyPointLocation, "Prva tajna.", true);
        tour.AddKeyPoint("Druga tačka", "Opis druge tačke.", "/images/druga.jpg", SecondKeyPointLocation, "Druga tajna.", false);
        return tour;
    }

    private static TourSession CreateActiveSession(Tour tour)
    {
        return new TourSession(WellKnownUsers.Explorer, tour.Id, FirstKeyPointLocation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.0004)]
    public void Tourist_within_50_meters_of_the_next_key_point_completes_it_with_the_arrival_time(double latitudeOffset)
    {
        var tour = CreateTourWithTwoKeyPoints();
        var session = CreateActiveSession(tour);
        var position = new Location(FirstKeyPointLocation.Latitude + latitudeOffset, FirstKeyPointLocation.Longitude);

        _keyPointArrival.Check(session, tour, position);

        var completion = session.CompletedKeyPoints.Should().ContainSingle().Subject;
        completion.KeyPointId.Should().Be(tour.KeyPoints[0].Id);
        completion.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        session.Status.Should().Be(TourSessionStatus.Active);
    }

    [Fact]
    public void Tourist_near_a_key_point_that_is_not_the_next_one_completes_no_key_point()
    {
        var tour = CreateTourWithTwoKeyPoints();
        var session = CreateActiveSession(tour);

        _keyPointArrival.Check(session, tour, SecondKeyPointLocation);

        session.CompletedKeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void Tourist_farther_than_50_meters_from_the_next_key_point_does_not_complete_it_but_the_activity_is_recorded()
    {
        var tour = CreateTourWithTwoKeyPoints();
        var session = CreateActiveSession(tour);
        var position = new Location(FirstKeyPointLocation.Latitude + 0.0006, FirstKeyPointLocation.Longitude);

        _keyPointArrival.Check(session, tour, position);

        session.CompletedKeyPoints.Should().BeEmpty();
        session.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Completing_the_last_key_point_completes_the_session_with_its_time()
    {
        var tour = CreateTourWithTwoKeyPoints();
        var session = CreateActiveSession(tour);
        session.CompleteKeyPoint(tour.KeyPoints[0].Id, false);

        _keyPointArrival.Check(session, tour, SecondKeyPointLocation);

        session.CompletedKeyPoints.Select(completion => completion.KeyPointId)
            .Should().Equal(tour.KeyPoints[0].Id, tour.KeyPoints[1].Id);
        session.Status.Should().Be(TourSessionStatus.Completed);
        session.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }
}
