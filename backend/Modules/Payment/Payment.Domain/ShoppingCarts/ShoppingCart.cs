using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public Guid BuyerId { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

    private ShoppingCart() { }

    public ShoppingCart(Guid buyerId) : base(Guid.NewGuid())
    {
        BuyerId = buyerId;
    }

    public int TotalPrice()
    {
        return _items.Sum(item => item.Price);
    }

    public void AddItem(Product product)
    {
        if (_items.Any(item => item.EntityId == product.EntityId))
        {
            throw new DomainException("The product is already in the cart.");
        }
        if (product.OwnerId == BuyerId)
        {
            throw new DomainException("You cannot buy your own product.");
        }

        _items.Add(new OrderItem(product.EntityId, product.Name, product.Price));
    }

    public void RemoveItem(Guid entityId)
    {
        var item = _items.FirstOrDefault(item => item.EntityId == entityId);
        if (item is null)
        {
            throw new NotFoundException($"Item {entityId} is not in the cart.");
        }

        _items.Remove(item);
    }
}
