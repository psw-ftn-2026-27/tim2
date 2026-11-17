namespace Payment.Domain.ShoppingCarts;

public sealed record OrderItem
{
    public Guid EntityId { get; private init; }
    public string Name { get; private init; }
    public int Price { get; private init; }

    private OrderItem()
    {
        Name = null!;
    }

    public OrderItem(Guid entityId, string name, int price)
    {
        EntityId = entityId;
        Name = name;
        Price = price;
    }
}
