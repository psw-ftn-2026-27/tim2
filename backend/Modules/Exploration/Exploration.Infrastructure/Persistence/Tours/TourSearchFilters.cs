using Exploration.Application.TourBrowsing;
using Exploration.Domain.Tours;

namespace Exploration.Infrastructure.Persistence.Tours;

// The search criteria shared by the tour list and the map search, so both narrow the tours the same way.
internal static class TourSearchFilters
{
    private const double EarthRadiusKm = 6371;
    private const double RadiansPerDegree = Math.PI / 180;

    public static IQueryable<Tour> ByCriteria(IQueryable<Tour> tours, TourSearchDto search)
    {
        tours = ByNameAndDifficulty(tours, search);
        tours = ByTagsAndTransport(tours, search);
        tours = ByPrice(tours, search);
        return ByLength(tours, search);
    }

    // Great-circle distance by the haversine formula, written with Math calls so that it runs in the database.
    // Math.Min keeps rounding from pushing the arcsine argument above 1 for points on opposite sides of the Earth.
    // Only key points visible before purchase count.
    public static IQueryable<Tour> ByArea(IQueryable<Tour> tours, SearchArea area)
    {
        var centerLatitude = area.Center.Latitude * RadiansPerDegree;
        var centerLongitude = area.Center.Longitude * RadiansPerDegree;
        var centerLatitudeCosine = Math.Cos(centerLatitude);
        var radiusKm = area.RadiusKm;

        return tours.Where(tour => tour.KeyPoints.Any(keyPoint =>
            keyPoint.IsVisibleBeforePurchase &&
            2 * EarthRadiusKm * Math.Asin(Math.Min(1, Math.Sqrt(
                Math.Pow(Math.Sin((keyPoint.Location.Latitude * RadiansPerDegree - centerLatitude) / 2), 2) +
                centerLatitudeCosine * Math.Cos(keyPoint.Location.Latitude * RadiansPerDegree) *
                Math.Pow(Math.Sin((keyPoint.Location.Longitude * RadiansPerDegree - centerLongitude) / 2), 2))))
            <= radiusKm));
    }

    private static IQueryable<Tour> ByNameAndDifficulty(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(name));
        }
        if (search.Difficulties is { Count: > 0 })
        {
            var difficulties = search.Difficulties;
            tours = tours.Where(tour => difficulties.Contains(tour.Difficulty));
        }
        return tours;
    }

    private static IQueryable<Tour> ByTagsAndTransport(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (search.Tags is { Count: > 0 })
        {
            var tags = search.Tags;
            tours = tours.Where(tour => tour.Tags.Any(tag => tags.Contains(tag)));
        }
        if (search.TransportModes is { Count: > 0 })
        {
            var transportModes = search.TransportModes;
            tours = tours.Where(tour => tour.TransportTimes.Any(time => transportModes.Contains(time.Transport)));
        }
        return tours;
    }

    private static IQueryable<Tour> ByPrice(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (search.MinPrice is not null)
        {
            var minPrice = search.MinPrice.Value;
            tours = tours.Where(tour => tour.Price >= minPrice);
        }
        if (search.MaxPrice is not null)
        {
            var maxPrice = search.MaxPrice.Value;
            tours = tours.Where(tour => tour.Price <= maxPrice);
        }
        return tours;
    }

    // A tour with an unknown length drops out of any range, because a comparison with NULL is never true in the database.
    private static IQueryable<Tour> ByLength(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (search.MinLengthKm is not null)
        {
            var minLengthKm = search.MinLengthKm.Value;
            tours = tours.Where(tour => tour.LengthKm >= minLengthKm);
        }
        if (search.MaxLengthKm is not null)
        {
            var maxLengthKm = search.MaxLengthKm.Value;
            tours = tours.Where(tour => tour.LengthKm <= maxLengthKm);
        }
        return tours;
    }
}
