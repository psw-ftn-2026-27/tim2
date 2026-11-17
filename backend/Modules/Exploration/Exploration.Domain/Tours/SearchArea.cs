using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record SearchArea
{
    public Location Center { get; }
    public double RadiusKm { get; }

    public SearchArea(Location center, double radiusKm)
    {
        if (double.IsNaN(radiusKm) || radiusKm <= 0)
        {
            throw new DomainException("A search area requires a positive radius in kilometres.");
        }

        Center = center;
        RadiusKm = radiusKm;
    }
}
