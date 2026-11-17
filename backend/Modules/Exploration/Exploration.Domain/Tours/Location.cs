using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Location
{
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
}
