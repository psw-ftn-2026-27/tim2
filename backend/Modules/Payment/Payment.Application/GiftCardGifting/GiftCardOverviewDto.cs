namespace Payment.Application.GiftCardGifting;

public sealed record GiftCardOverviewDto(List<GiftCardDto> Received, List<GiftCardDto> Sent);
