using Microsoft.EntityFrameworkCore;
using Payment.Application.TourPurchaseTokens;
using Payment.Domain.TourPurchaseTokens;

namespace Payment.Infrastructure.Persistence.TourPurchaseTokens;

internal sealed class TourPurchaseTokenRepository : ITourPurchaseTokenRepository
{
    private readonly PaymentDbContext _dbContext;

    public TourPurchaseTokenRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid buyerId, Guid tourId)
    {
        return _dbContext.TourPurchaseTokens.AnyAsync(token => token.BuyerId == buyerId && token.TourId == tourId);
    }

    public void Add(TourPurchaseToken token)
    {
        _dbContext.TourPurchaseTokens.Add(token);
    }
}
