namespace Payment.Application.GiftCardGifting;

public sealed record PurchaseGiftCardDto(string RecipientEmail, int Amount, string? Message);
