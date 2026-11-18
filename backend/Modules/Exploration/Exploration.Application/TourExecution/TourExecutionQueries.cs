using Exploration.Application.TourSessions;

namespace Exploration.Application.TourExecution;

public sealed class TourExecutionQueries
{
    private readonly ITourSessionReadRepository _sessionReadRepository;

    public TourExecutionQueries(ITourSessionReadRepository sessionReadRepository)
    {
        _sessionReadRepository = sessionReadRepository;
    }

    public Task<ActiveTourDto?> GetActiveTourAsync(Guid touristId, Guid tourId)
    {
        return _sessionReadRepository.GetActiveTourAsync(touristId, tourId);
    }
}
