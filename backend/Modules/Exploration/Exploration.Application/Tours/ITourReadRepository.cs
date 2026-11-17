using Exploration.Application.KeyPointAuthoring;
using Exploration.Contracts;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize);
    Task<PublishedTourSummary?> GetPublishedSummaryAsync(Guid id);
    Task<List<FeaturedTourDto>> GetFeaturedAsync(int count);
    Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId);
}
