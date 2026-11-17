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

    public void AddItem(Product product, bool isAlreadyPurchased)
    {
        if (isAlreadyPurchased)
        {
            throw new DomainException("You have already purchased this tour.");
        }
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

    public List<OrderItem> Checkout(IReadOnlyCollection<Guid> publishedTourIds)
    {
        if (_items.Count == 0)
        {
            throw new DomainException("An empty cart cannot be purchased.");
        }
        var unavailableItem = _items.FirstOrDefault(item => !publishedTourIds.Contains(item.EntityId));
        if (unavailableItem is not null)
        {
            throw new DomainException($"The tour {unavailableItem.Name} is no longer available.");
        }

        var purchasedItems = _items.ToList();
        _items.Clear();
        return purchasedItems;
    }
}
