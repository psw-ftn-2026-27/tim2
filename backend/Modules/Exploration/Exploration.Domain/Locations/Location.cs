using Shared.Domain.Exceptions;

namespace Exploration.Domain.Locations;

public sealed record Location
{
    private const double EarthRadiusInMeters = 6371000;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("A location requires a latitude from -90 to 90.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("A location requires a longitude from -180 to 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    // Haversine formula: the distance along the surface of the Earth, treated as a sphere.
    public double DistanceInMetersTo(Location other)
    {
        var latitudeDifference = ToRadians(other.Latitude - Latitude);
        var longitudeDifference = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

        return EarthRadiusInMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
