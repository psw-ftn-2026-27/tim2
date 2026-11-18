using Exploration.Application.TourSessions;
using Exploration.Domain.TourSessions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourSessions;

internal sealed class TourSessionRepository : ITourSessionRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourSessionRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourSession?> GetActiveByTouristAsync(Guid touristId)
    {
        return _dbContext.TourSessions.FirstOrDefaultAsync(session =>
            session.TouristId == touristId && session.Status == TourSessionStatus.Active);
    }

    public Task<TourSession?> GetActiveByTouristAndTourAsync(Guid touristId, Guid tourId)
    {
        return _dbContext.TourSessions.FirstOrDefaultAsync(session =>
            session.TouristId == touristId && session.TourId == tourId && session.Status == TourSessionStatus.Active);
    }

    public void Add(TourSession session)
    {
        _dbContext.TourSessions.Add(session);
    }
}
