using Exploration.Domain.TourSessions;

namespace Exploration.Application.TourSessions;

public interface ITourSessionRepository
{
    Task<TourSession?> GetActiveByTouristAsync(Guid touristId);
    Task<TourSession?> GetActiveByTouristAndTourAsync(Guid touristId, Guid tourId);
    void Add(TourSession session);
}
