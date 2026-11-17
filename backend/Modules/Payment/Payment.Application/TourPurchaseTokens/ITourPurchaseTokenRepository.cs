using Payment.Domain.TourPurchaseTokens;

namespace Payment.Application.TourPurchaseTokens;

public interface ITourPurchaseTokenRepository
{
    void Add(TourPurchaseToken token);
}
