using System.Linq.Expressions;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Contracts;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    // Difficulty is stored as text, so the order is spelled out instead of sorting alphabetically.
    private static readonly Expression<Func<Tour, int>> DifficultyRank =
        tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2;

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
        var published = TourSearchFilters.ByCriteria(_dbContext.Tours.Where(tour => tour.Status == TourStatus.Published), search);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(Sort(published, search))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    // Only key points visible before purchase count, both for the area (in TourSearchFilters) and for the map marker.
    public async Task<PageResult<LocatedTourDto>> GetPublishedByLocationAsync(SearchArea area, TourSearchDto search, int page, int pageSize)
    {
        var inArea = TourSearchFilters.ByArea(_dbContext.Tours.Where(tour => tour.Status == TourStatus.Published), area);
        var published = TourSearchFilters.ByCriteria(inArea, search);

        var totalCount = await published.CountAsync();
        var sorted = published.OrderByDescending(tour => tour.PublishedAt).ThenBy(tour => tour.Id);
        var items = await ProjectToLocatedDtos(sorted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LocatedTourDto>(items, totalCount);
    }

    public Task<PublishedTourSummary?> GetPublishedSummaryAsync(Guid id)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == id && tour.Status == TourStatus.Published)
            .Select(tour => new PublishedTourSummary(tour.Id, tour.Name, tour.AuthorId, tour.Price))
            .FirstOrDefaultAsync();
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
                    .ToList(),
                tour.LengthKm))
            .FirstOrDefaultAsync();
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
                tour.ArchivedAt,
                tour.LengthKm));
    }
}
