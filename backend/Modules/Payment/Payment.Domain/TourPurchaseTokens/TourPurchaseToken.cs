using Shared.Domain;

namespace Payment.Domain.TourPurchaseTokens;

public sealed class TourPurchaseToken : AggregateRoot
{
    public Guid BuyerId { get; private set; }
    public Guid TourId { get; private set; }
    public string Name { get; private set; }
    public int Price { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private TourPurchaseToken()
    {
        Name = null!;
    }

    public TourPurchaseToken(Guid buyerId, Guid tourId, string name, int price) : base(Guid.NewGuid())
    {
        BuyerId = buyerId;
        TourId = tourId;
        Name = name;
        Price = price;
        PurchasedAt = DateTime.UtcNow;
    }
}
