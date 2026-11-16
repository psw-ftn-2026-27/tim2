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

    public async Task<PublishedTourSummary?> GetPublishedTourAsync(Guid tourId)
    {
        var tour = await _tourReadRepository.GetPublishedByIdAsync(tourId);
        if (tour is null)
        {
            return null;
        }

        return new PublishedTourSummary(tour.Id, tour.Name, tour.AuthorId, tour.Price);
    }
}
