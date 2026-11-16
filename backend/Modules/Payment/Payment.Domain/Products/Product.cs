using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

public sealed class Product : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public int Price { get; private set; }

    private Product()
    {
        Name = null!;
    }

    public Product(Guid entityId, string name, Guid ownerId, int price) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A product requires a name.");
        }
        if (price < 0)
        {
            throw new DomainException("A product price cannot be negative.");
        }

        EntityId = entityId;
        Name = name;
        OwnerId = ownerId;
        Price = price;
    }
}
