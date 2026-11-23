using System.Security.Cryptography;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class Coupon : AggregateRoot
{
    public const int CodeLength = 8;
    public const int MinPercent = 1;
    public const int MaxPercent = 100;

    private const string CodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public Guid OwnerId { get; private set; }
    public string Code { get; private set; }
    public int Percent { get; private set; }
    public DateOnly ExpiresOn { get; private set; }
    public Guid? ProductEntityId { get; private set; }

    private Coupon()
    {
        Code = null!;
    }

    public Coupon(Guid ownerId, Guid? productEntityId, Guid? productOwnerId, int percent, DateOnly expiresOn, DateTime now) : base(Guid.NewGuid())
    {
        if (productEntityId is not null && productOwnerId != ownerId)
        {
            throw new DomainException("A coupon can only be bound to a product its owner owns.");
        }
        if (percent < MinPercent || percent > MaxPercent)
        {
            throw new DomainException($"A coupon discount must be between {MinPercent} and {MaxPercent} percent.");
        }
        if (expiresOn <= DateOnly.FromDateTime(now))
        {
            throw new DomainException("A coupon must expire after today.");
        }

        OwnerId = ownerId;
        ProductEntityId = productEntityId;
        Percent = percent;
        ExpiresOn = expiresOn;
        Code = RandomNumberGenerator.GetString(CodeCharacters, CodeLength);
    }
}
