using Exploration.Application.TourExecution;
using Exploration.Application.TourSessions;
using Exploration.Domain.TourSessions;
using Exploration.Domain.Tours;
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
        var sessions = _dbContext.TourSessions
            .Where(session => session.TouristId == touristId && session.TourId == tourId)
            .OrderByDescending(session => session.StartedAt);
        return ProjectToProgress(sessions, _dbContext.Tours).FirstOrDefaultAsync();
    }

    private static IQueryable<TourProgressDto> ProjectToProgress(IQueryable<TourSession> sessions, IQueryable<Tour> tours)
    {
        return sessions.Join(tours, session => session.TourId, tour => tour.Id, (session, tour) => new TourProgressDto(
            tour.Name,
            session.Status,
            tour.KeyPoints
                .Where(keyPoint => !session.CompletedKeyPoints.Any(completion => completion.KeyPointId == keyPoint.Id))
                .OrderBy(keyPoint => keyPoint.Order)
                .Select(keyPoint => new TourProgressKeyPointDto(keyPoint.Name, keyPoint.Location.Latitude, keyPoint.Location.Longitude))
                .FirstOrDefault(),
            tour.KeyPoints
                .Where(keyPoint => session.CompletedKeyPoints.Any(completion => completion.KeyPointId == keyPoint.Id))
                .OrderBy(keyPoint => keyPoint.Order)
                .Select(keyPoint => new CompletedKeyPointDto(keyPoint.Name, keyPoint.Secret))
                .ToList()));
    }
}
