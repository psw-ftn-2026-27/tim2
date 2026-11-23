using Microsoft.EntityFrameworkCore;
using Payment.Application.PurchaseViewing;
using Payment.Application.TourPurchaseTokens;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.TourPurchaseTokens;

internal sealed class TourPurchaseTokenReadRepository : ITourPurchaseTokenReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public TourPurchaseTokenReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<PurchasedTourDto>> GetByBuyerIdAsync(Guid buyerId, int page, int pageSize)
    {
        var tokens = _dbContext.TourPurchaseTokens
            .Where(token => token.BuyerId == buyerId)
            .OrderByDescending(token => token.PurchasedAt);

        var totalCount = await tokens.CountAsync();
        var items = await tokens
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(token => new PurchasedTourDto(
                token.TourId,
                token.Name,
                token.Price,
                token.PurchasedAt))
            .ToListAsync();

        return new PageResult<PurchasedTourDto>(items, totalCount);
    }

    public Task<bool> ExistsAsync(Guid buyerId, Guid tourId)
    {
        return _dbContext.TourPurchaseTokens
            .AnyAsync(token => token.BuyerId == buyerId && token.TourId == tourId);
    }
}
