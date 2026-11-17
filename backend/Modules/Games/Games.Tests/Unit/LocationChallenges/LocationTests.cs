using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationTests
{
    [Theory]
    [InlineData(-90.0001, 0)]
    [InlineData(90.0001, 0)]
    [InlineData(0, -180.0001)]
    [InlineData(0, 180.0001)]
    public void Location_with_a_latitude_or_longitude_outside_the_allowed_range_cannot_exist(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    // On a sphere of radius 6371 km one degree along a meridian is 111 195 m; along a parallel it shrinks
    // by the cosine of the latitude (0.704 at Trg slobode in Novi Sad, latitude 45.2551).
    [Theory]
    [InlineData(45.0, 19.0, 46.0, 19.0, 111195)]
    [InlineData(45.2551, 19.8451, 45.2561, 19.8451, 111.2)]
    [InlineData(45.2551, 19.8451, 45.2551, 19.8551, 782.8)]
    public void Distance_between_two_known_locations_matches_the_measured_distance_in_meters(
        double fromLatitude, double fromLongitude, double toLatitude, double toLongitude, double expectedMeters)
    {
        var from = new Location(fromLatitude, fromLongitude);
        var to = new Location(toLatitude, toLongitude);

        var distance = from.DistanceInMetersTo(to);

        distance.Should().BeApproximately(expectedMeters, 1);
    }
}
