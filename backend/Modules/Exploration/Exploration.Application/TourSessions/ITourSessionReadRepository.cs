using Exploration.Application.TourExecution;

namespace Exploration.Application.TourSessions;

public interface ITourSessionReadRepository
{
    Task<TourProgressDto?> GetTourProgressAsync(Guid touristId, Guid tourId);
}
