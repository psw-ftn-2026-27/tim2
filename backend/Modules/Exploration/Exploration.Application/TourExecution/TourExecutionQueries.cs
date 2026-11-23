using Exploration.Application.TourSessions;

namespace Exploration.Application.TourExecution;

public sealed class TourExecutionQueries
{
    private readonly ITourSessionReadRepository _sessionReadRepository;

    public TourExecutionQueries(ITourSessionReadRepository sessionReadRepository)
    {
        _sessionReadRepository = sessionReadRepository;
    }

    public Task<TourProgressDto?> GetTourProgressAsync(Guid touristId, Guid tourId)
    {
        return _sessionReadRepository.GetTourProgressAsync(touristId, tourId);
    }
}
