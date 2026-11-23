using FluentAssertions;
using Positions.Domain.UserPositions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Positions.Tests.Unit.UserPositions;

public class CoordinatesTests
{
    [Theory]
    [InlineData(90.0001, 0)]
    [InlineData(-90.0001, 0)]
    [InlineData(0, 180.0001)]
    [InlineData(0, -180.0001)]
    public void Coordinates_outside_the_latitude_or_longitude_bounds_are_rejected(double latitude, double longitude)
    {
        var create = () => new Coordinates(latitude, longitude);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(90, -180)]
    [InlineData(-90, 180)]
    public void Coordinates_on_the_latitude_and_longitude_bounds_are_accepted(double latitude, double longitude)
    {
        var coordinates = new Coordinates(latitude, longitude);

        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }
}
