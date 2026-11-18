using Exploration.Application.TourExecution;
using Exploration.Application.TourSessions;
using Exploration.Domain.TourSessions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourSessions;

internal sealed class TourSessionReadRepository : ITourSessionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourSessionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Progress through the key points is not recorded yet, so the next uncompleted key point is the first one.
    public Task<ActiveTourDto?> GetActiveTourAsync(Guid touristId, Guid tourId)
    {
        return _dbContext.TourSessions
            .Where(session => session.TouristId == touristId && session.TourId == tourId && session.Status == TourSessionStatus.Active)
            .Join(_dbContext.Tours, session => session.TourId, tour => tour.Id, (session, tour) => tour)
            .Select(tour => new ActiveTourDto(
                tour.Name,
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new ActiveTourKeyPointDto(
                        keyPoint.Name,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude))
                    .First()))
            .FirstOrDefaultAsync();
    }
}
