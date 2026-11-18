using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardGifting;

public sealed record GiftCardDto(Guid Id, int Amount, string Message, string OtherPartyEmail, DateTime ExpiresAt, GiftCardStatus Status);
