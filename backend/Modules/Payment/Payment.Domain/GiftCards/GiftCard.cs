using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCard : AggregateRoot
{
    public const int MinAmount = 10;
    public const int MaxAmount = 1000;
    public const int MaxMessageLength = 200;
    public const int ValidityInYears = 3;

    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public int Amount { get; private set; }
    public string Message { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private GiftCard()
    {
        Message = null!;
    }

    public GiftCard(Guid senderId, Guid recipientId, int amount, string? message, DateTime now) : base(Guid.NewGuid())
    {
        if (senderId == recipientId)
        {
            throw new DomainException("A gift card cannot be sent to yourself.");
        }
        if (amount < MinAmount || amount > MaxAmount)
        {
            throw new DomainException($"A gift card amount must be between {MinAmount} and {MaxAmount} EC.");
        }
        if (message is not null && message.Length > MaxMessageLength)
        {
            throw new DomainException($"A gift card message can have at most {MaxMessageLength} characters.");
        }

        SenderId = senderId;
        RecipientId = recipientId;
        Amount = amount;
        Message = message ?? string.Empty;
        CreatedAt = now;
        ExpiresAt = now.AddYears(ValidityInYears);
    }

    public GiftCardStatus CurrentStatus(DateTime now)
    {
        if (UsedAt is not null)
        {
            return GiftCardStatus.Used;
        }

        return now > ExpiresAt ? GiftCardStatus.Expired : GiftCardStatus.Valid;
    }
}
