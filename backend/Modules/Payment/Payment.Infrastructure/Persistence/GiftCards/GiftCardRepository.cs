using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardRepository : IGiftCardRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<GiftCard>> GetBySenderOrRecipientAsync(Guid userId)
    {
        return _dbContext.GiftCards
            .Where(giftCard => giftCard.SenderId == userId || giftCard.RecipientId == userId)
            .ToListAsync();
    }

    public void Add(GiftCard giftCard)
    {
        _dbContext.GiftCards.Add(giftCard);
    }
}
