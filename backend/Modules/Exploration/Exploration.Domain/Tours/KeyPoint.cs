using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public Location Location { get; private set; }
    public string? Secret { get; private set; }
    public bool IsVisibleBeforePurchase { get; private set; }
    public int Order { get; private set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        ImageUrl = null!;
        Location = null!;
    }

    internal KeyPoint(int order, string name, string description, string imageUrl, Location location,
        string? secret, bool isVisibleBeforePurchase)
        : base(Guid.NewGuid())
    {
        Validate(name, description, imageUrl);

        Order = order;
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        Location = location;
        Secret = secret;
        IsVisibleBeforePurchase = isVisibleBeforePurchase;
    }

    internal void Change(string name, string description, string imageUrl, Location location,
        string? secret, bool isVisibleBeforePurchase)
    {
        Validate(name, description, imageUrl);

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        Location = location;
        Secret = secret;
        IsVisibleBeforePurchase = isVisibleBeforePurchase;
    }

    internal void ChangeOrder(int order)
    {
        Order = order;
    }

    private static void Validate(string name, string description, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A key point requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A key point requires a description.");
        }
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new DomainException("A key point requires an image.");
        }
    }
}
