using Exploration.Application.Tours;
using Exploration.Contracts;

namespace Exploration.Application;

public sealed class ExplorationApi : IExplorationApi
{
    private readonly ITourReadRepository _tourReadRepository;

    public ExplorationApi(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<PublishedTourSummary?> GetPublishedTourAsync(Guid tourId)
    {
        return _tourReadRepository.GetPublishedSummaryAsync(tourId);
    }
}
