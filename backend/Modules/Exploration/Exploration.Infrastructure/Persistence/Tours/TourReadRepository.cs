using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
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

        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.ToLower();
            published = published.Where(tour => tour.Name.ToLower().Contains(name));
        }
        if (search.Difficulty is not null)
        {
            var difficulty = search.Difficulty.Value;
            published = published.Where(tour => tour.Difficulty == difficulty);
        }
        if (search.Tags is { Count: > 0 })
        {
            var tags = search.Tags;
            published = published.Where(tour => tour.Tags.Any(tag => tags.Contains(tag)));
        }
        if (search.TransportModes is { Count: > 0 })
        {
            var transportModes = search.TransportModes;
            published = published.Where(tour => tour.TransportTimes.Any(time => transportModes.Contains(time.Transport)));
        }

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(Sort(published, search.DifficultySort))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    private static IQueryable<Tour> Sort(IQueryable<Tour> tours, DifficultySort difficultySort)
    {
        // Difficulty is stored as text, so the order is spelled out instead of sorting alphabetically.
        return difficultySort switch
        {
            DifficultySort.Ascending => tours
                .OrderBy(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            DifficultySort.Descending => tours
                .OrderByDescending(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            _ => tours.OrderByDescending(tour => tour.PublishedAt)
        };
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
                    : new RecommendedGroupSizeDto(tour.RecommendedGroupSize.Min, tour.RecommendedGroupSize.Max)));
    }
}
