using Exploration.Domain.Locations;
using Exploration.Domain.TourSessions;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourSessions;

public class TourStartingTests
{
    private static readonly Guid Tourist = WellKnownUsers.Explorer;
    private static readonly Guid Tour = Guid.NewGuid();
    private static readonly Guid OtherTour = Guid.NewGuid();
    private static readonly Location Position = new(45.2671, 19.8335);

    private readonly TourStarting _tourStarting = new();

    private static TourSession CreateAbandonedSession(Guid tourId)
    {
        var session = new TourSession(Tourist, tourId, Position);
        session.Abandon();
        return session;
    }

    [Fact]
    public void Tour_that_is_not_purchased_cannot_be_started()
    {
        var starting = () => _tourStarting.Start(Tourist, Tour, false, Position, null);

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_cannot_be_started_without_the_tourists_position()
    {
        var starting = () => _tourStarting.Start(Tourist, Tour, true, null, null);

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Starting_a_tour_that_is_already_active_returns_the_existing_session()
    {
        var activeSession = new TourSession(Tourist, Tour, Position);

        var session = _tourStarting.Start(Tourist, Tour, true, Position, activeSession);

        session.Should().BeSameAs(activeSession);
    }

    [Fact]
    public void Starting_another_tour_while_one_is_active_is_rejected()
    {
        var activeSession = new TourSession(Tourist, OtherTour, Position);

        var starting = () => _tourStarting.Start(Tourist, Tour, true, Position, activeSession);

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tourist_whose_previous_session_was_abandoned_gets_a_new_active_session()
    {
        var abandonedSession = CreateAbandonedSession(Tour);

        var session = _tourStarting.Start(Tourist, Tour, true, Position, abandonedSession);

        session.Should().NotBeSameAs(abandonedSession);
        session.Status.Should().Be(TourSessionStatus.Active);
        session.TouristId.Should().Be(Tourist);
        session.TourId.Should().Be(Tour);
    }
}
