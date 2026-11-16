using Exploration.Application.Tours;
using Shared.Domain;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private const int FeaturedTourCount = 3;

    private readonly ITourReadRepository _tourReadRepository;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(search, page, pageSize);
    }

    public Task<List<FeaturedTourDto>> GetFeaturedAsync()
    {
        return _tourReadRepository.GetFeaturedAsync(FeaturedTourCount);
    }
}
