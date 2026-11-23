using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardRepository
{
    Task<List<GiftCard>> GetBySenderOrRecipientAsync(Guid userId);

    void Add(GiftCard giftCard);
}
