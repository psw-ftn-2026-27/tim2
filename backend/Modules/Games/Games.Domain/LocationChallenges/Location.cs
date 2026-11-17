using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record Location
{
    private const double MinimumLatitude = -90;
    private const double MaximumLatitude = 90;
    private const double MinimumLongitude = -180;
    private const double MaximumLongitude = 180;
    private const double EarthRadiusInMeters = 6_371_000;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        // Asks "is the value inside the bounds?" rather than "is it outside?", because every
        // comparison with double.NaN is false: an outside check would let NaN through.
        if (!(latitude >= MinimumLatitude && latitude <= MaximumLatitude))
        {
            throw new DomainException($"A latitude must be from {MinimumLatitude} to {MaximumLatitude}.");
        }
        if (!(longitude >= MinimumLongitude && longitude <= MaximumLongitude))
        {
            throw new DomainException($"A longitude must be from {MinimumLongitude} to {MaximumLongitude}.");
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
