using Exploration.Application.TourExecution;

namespace Exploration.Application.TourSessions;

public interface ITourSessionReadRepository
{
    Task<ActiveTourDto?> GetActiveTourAsync(Guid touristId, Guid tourId);
}
