using System.Linq.Expressions;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    // Difficulty is stored as text, so the order is spelled out instead of sorting alphabetically.
    private static readonly Expression<Func<Tour, int>> DifficultyRank =
        tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2;

    private const double EarthRadiusKm = 6371;
    private const double RadiansPerDegree = Math.PI / 180;

    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize)
    {
        var published = _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);
        published = FilterByNameAndDifficulty(published, search);
        published = FilterByTagsAndTransport(published, search);
        published = FilterByPrice(published, search);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(Sort(published, search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    // Only key points visible before purchase count, both for the area and for the map marker.
    public async Task<PageResult<LocatedTourDto>> GetPublishedByLocationAsync(SearchArea area, TourSearchDto search, int page, int pageSize)
    {
        var published = _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);
        published = FilterByArea(published, area);
        published = FilterByNameAndDifficulty(published, search);
        published = FilterByTagsAndTransport(published, search);
        published = FilterByPrice(published, search);

        var totalCount = await published.CountAsync();
        var sorted = published.OrderByDescending(tour => tour.PublishedAt).ThenBy(tour => tour.Id);
        var items = await ProjectToLocatedDtos(sorted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LocatedTourDto>(items, totalCount);
    }

    // For now the newest tours are featured; reactivating an archived tour keeps its first publishing time.
    public Task<List<FeaturedTourDto>> GetFeaturedAsync(int count)
    {
        return _dbContext.Tours
            .Where(tour => tour.Status == TourStatus.Published)
            .OrderByDescending(tour => tour.PublishedAt)
            .ThenBy(tour => tour.Id)
            .Take(count)
            .Select(tour => new FeaturedTourDto(tour.Name, tour.Description, tour.Price))
            .ToListAsync();
    }

    public Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.AuthorId == authorId)
            .Select(tour => new TourKeyPointsDto(
                tour.Name,
                tour.Status,
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new KeyPointDto(
                        keyPoint.Id,
                        keyPoint.Order,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.ImageUrl,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude,
                        keyPoint.Secret,
                        keyPoint.IsVisibleBeforePurchase))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    // Great-circle distance by the haversine formula, written with Math calls so that it runs in the database.
    // Math.Min keeps rounding from pushing the arcsine argument above 1 for points on opposite sides of the Earth.
    private static IQueryable<Tour> FilterByArea(IQueryable<Tour> tours, SearchArea area)
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

    private static IQueryable<Tour> FilterByNameAndDifficulty(IQueryable<Tour> tours, TourSearchDto search)
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

    private static IQueryable<Tour> FilterByTagsAndTransport(IQueryable<Tour> tours, TourSearchDto search)
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

    private static IQueryable<Tour> FilterByPrice(IQueryable<Tour> tours, TourSearchDto search)
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

    // Difficulty comes first and price decides within the same difficulty; the newest tour wins every remaining tie.
    private static IQueryable<Tour> Sort(IQueryable<Tour> tours, TourSearchDto search)
    {
        var sorted = search.DifficultySort switch
        {
            SortDirection.Ascending => tours.OrderBy(DifficultyRank),
            SortDirection.Descending => tours.OrderByDescending(DifficultyRank),
            _ => tours.OrderBy(tour => 0)
        };
        sorted = search.PriceSort switch
        {
            SortDirection.Ascending => sorted.ThenBy(tour => tour.Price),
            SortDirection.Descending => sorted.ThenByDescending(tour => tour.Price),
            _ => sorted
        };
        return sorted.ThenByDescending(tour => tour.PublishedAt);
    }

    // The marker stands on the first visible key point, even when that point lies outside the search area.
    private static IQueryable<LocatedTourDto> ProjectToLocatedDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new LocatedTourDto(
                tour.Id,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Price,
                tour.KeyPoints
                    .Where(keyPoint => keyPoint.IsVisibleBeforePurchase)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => keyPoint.Location.Latitude)
                    .First(),
                tour.KeyPoints
                    .Where(keyPoint => keyPoint.IsVisibleBeforePurchase)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => keyPoint.Location.Longitude)
                    .First()));
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.RecommendedGroupSize == null
                    ? null
                    : new RecommendedGroupSizeDto(tour.RecommendedGroupSize.Min, tour.RecommendedGroupSize.Max),
                tour.Price,
                tour.ArchivedAt));
    }
}
