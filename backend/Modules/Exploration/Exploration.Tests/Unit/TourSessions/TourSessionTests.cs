using Exploration.Domain.Locations;
using Exploration.Domain.TourSessions;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourSessions;

public class TourSessionTests
{
    [Fact]
    public void Session_that_is_not_active_cannot_be_abandoned()
    {
        var session = new TourSession(WellKnownUsers.Explorer, Guid.NewGuid(), new Location(45.2671, 19.8335));
        session.Abandon();

        var abandoning = () => session.Abandon();

        abandoning.Should().Throw<DomainException>();
    }
}
