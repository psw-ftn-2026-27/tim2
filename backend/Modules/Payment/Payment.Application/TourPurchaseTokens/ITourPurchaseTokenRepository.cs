using Payment.Domain.TourPurchaseTokens;

namespace Payment.Application.TourPurchaseTokens;

public interface ITourPurchaseTokenRepository
{
    Task<bool> ExistsAsync(Guid buyerId, Guid tourId);
    void Add(TourPurchaseToken token);
}
