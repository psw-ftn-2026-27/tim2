using Exploration.Application.Tours;
using Exploration.Domain.Locations;
using Exploration.Domain.TourLocationSearchRecords;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private const int FeaturedTourCount = 3;

    private readonly ITourReadRepository _tourReadRepository;
    private readonly IPaymentApi _paymentApi;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository, IPaymentApi paymentApi)
    {
        _tourReadRepository = tourReadRepository;
        _paymentApi = paymentApi;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(TourSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(search, page, pageSize);
    }

    public Task<PageResult<LocatedTourDto>> GetPublishedByLocationAsync(TourLocationSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var area = new SearchArea(new Location(search.Latitude, search.Longitude), search.RadiusKm);
        var criteria = new TourSearchDto(search.Name, search.Difficulties, search.Tags, search.TransportModes, search.MinPrice, search.MaxPrice, search.MinLengthKm, search.MaxLengthKm);

        return _tourReadRepository.GetPublishedByLocationAsync(area, criteria, page, pageSize);
    }

    public Task<List<FeaturedTourDto>> GetFeaturedAsync()
    {
        return _tourReadRepository.GetFeaturedAsync(FeaturedTourCount);
    }

    // A draft is never open. The author opens their published tour without asking Payment; everyone else
    // opens a published tour, and an archived one only after purchasing it.
    public async Task<TourDetailsDto> GetTourDetailsAsync(Guid tourId, Guid userId)
    {
        var access = await _tourReadRepository.GetAccessAsync(tourId);
        if (access is null || access.Status == TourStatus.Draft)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        var isOwnTour = access.AuthorId == userId;
        var isPurchased = false;
        if (!isOwnTour || access.Status != TourStatus.Published)
        {
            isPurchased = await _paymentApi.HasPurchasedAsync(userId, tourId);
        }

        if (access.Status == TourStatus.Archived && !isPurchased)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        var details = await _tourReadRepository.GetDetailsAsync(tourId, isPurchased, isOwnTour);
        if (details is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        return details;
    }
}
