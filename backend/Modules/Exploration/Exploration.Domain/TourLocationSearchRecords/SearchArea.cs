using Exploration.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourLocationSearchRecords;

public sealed record SearchArea
{
    public Location Center { get; private init; }
    public double RadiusKm { get; private init; }

    private SearchArea()
    {
        Center = null!;
    }

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
