using System.Linq.Expressions;
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

    private static IQueryable<Tour> FilterByNameAndDifficulty(IQueryable<Tour> tours, TourSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(name));
        }
        if (search.Difficulty is not null)
        {
            var difficulty = search.Difficulty.Value;
            tours = tours.Where(tour => tour.Difficulty == difficulty);
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
                tour.Price));
    }
}
