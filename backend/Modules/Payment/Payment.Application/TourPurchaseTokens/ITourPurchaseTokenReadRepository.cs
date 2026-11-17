using Payment.Application.PurchaseViewing;
using Shared.Domain;

namespace Payment.Application.TourPurchaseTokens;

public interface ITourPurchaseTokenReadRepository
{
    Task<PageResult<PurchasedTourDto>> GetByBuyerIdAsync(Guid buyerId, int page, int pageSize);
}
