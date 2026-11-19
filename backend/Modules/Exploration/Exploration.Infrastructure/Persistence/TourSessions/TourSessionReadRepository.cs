using Exploration.Application.TourExecution;
using Exploration.Application.TourSessions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourSessions;

internal sealed class TourSessionReadRepository : ITourSessionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourSessionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The tourist's latest session for the tour, whatever its status, so a completed tour is shown as completed.
    // Secrets come only with completed key points (P4); a completed tour has no next key point left.
    public Task<TourProgressDto?> GetTourProgressAsync(Guid touristId, Guid tourId)
    {
        return _dbContext.TourSessions
            .Where(session => session.TouristId == touristId && session.TourId == tourId)
            .OrderByDescending(session => session.StartedAt)
            .Join(_dbContext.Tours, session => session.TourId, tour => tour.Id, (session, tour) => new { session, tour })
            .Select(row => new TourProgressDto(
                row.tour.Name,
                row.session.Status,
                row.tour.KeyPoints
                    .Where(keyPoint => !row.session.CompletedKeyPoints.Any(completion => completion.KeyPointId == keyPoint.Id))
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new TourProgressKeyPointDto(
                        keyPoint.Name,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude))
                    .FirstOrDefault(),
                row.tour.KeyPoints
                    .Where(keyPoint => row.session.CompletedKeyPoints.Any(completion => completion.KeyPointId == keyPoint.Id))
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new CompletedKeyPointDto(keyPoint.Name, keyPoint.Secret))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
