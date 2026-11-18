using Exploration.Domain.Locations;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Locations;

public class LocationTests
{
    [Theory]
    [InlineData(90.5, 0)]
    [InlineData(-90.5, 0)]
    [InlineData(0, 180.5)]
    [InlineData(0, -180.5)]
    public void Location_requires_a_latitude_from_minus_90_to_90_and_a_longitude_from_minus_180_to_180(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }
}
