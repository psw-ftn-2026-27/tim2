using Payment.Application.TourPurchaseTokens;
using Shared.Domain;

namespace Payment.Application.PurchaseViewing;

public sealed class PurchaseViewingQueries
{
    private readonly ITourPurchaseTokenReadRepository _tokenReadRepository;

    public PurchaseViewingQueries(ITourPurchaseTokenReadRepository tokenReadRepository)
    {
        _tokenReadRepository = tokenReadRepository;
    }

    public Task<PageResult<PurchasedTourDto>> GetPurchasedToursAsync(Guid buyerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tokenReadRepository.GetByBuyerIdAsync(buyerId, page, pageSize);
    }
}
