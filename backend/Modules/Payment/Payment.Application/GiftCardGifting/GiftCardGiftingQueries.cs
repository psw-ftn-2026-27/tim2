using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardGifting;

public sealed class GiftCardGiftingQueries
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardGiftingQueries(IGiftCardRepository giftCardRepository, IIdentityApi identityApi)
    {
        _giftCardRepository = giftCardRepository;
        _identityApi = identityApi;
    }

    public async Task<GiftCardOverviewDto> GetOverviewAsync(Guid userId)
    {
        var giftCards = await _giftCardRepository.GetBySenderOrRecipientAsync(userId);
        var now = DateTime.UtcNow;

        var received = new List<GiftCardDto>();
        var sent = new List<GiftCardDto>();
        foreach (var giftCard in giftCards.OrderByDescending(giftCard => giftCard.CreatedAt))
        {
            if (giftCard.RecipientId == userId)
            {
                received.Add(await ToDtoAsync(giftCard, giftCard.SenderId, now));
            }
            if (giftCard.SenderId == userId)
            {
                sent.Add(await ToDtoAsync(giftCard, giftCard.RecipientId, now));
            }
        }

        return new GiftCardOverviewDto(received, sent);
    }

    private async Task<GiftCardDto> ToDtoAsync(GiftCard giftCard, Guid otherPartyId, DateTime now)
    {
        var otherParty = await _identityApi.GetExplorerAsync(otherPartyId);

        return new GiftCardDto(
            giftCard.Id,
            giftCard.Amount,
            giftCard.Message,
            otherParty?.Email ?? string.Empty,
            giftCard.ExpiresAt,
            giftCard.CurrentStatus(now));
    }
}
