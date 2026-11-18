using Payment.Application.TourPurchaseTokens;
using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ITourPurchaseTokenReadRepository _tourPurchaseTokenReadRepository;

    public PaymentApi(ITourPurchaseTokenReadRepository tourPurchaseTokenReadRepository)
    {
        _tourPurchaseTokenReadRepository = tourPurchaseTokenReadRepository;
    }

    public Task<bool> HasPurchasedAsync(Guid buyerId, Guid tourId)
    {
        return _tourPurchaseTokenReadRepository.ExistsAsync(buyerId, tourId);
    }
}
