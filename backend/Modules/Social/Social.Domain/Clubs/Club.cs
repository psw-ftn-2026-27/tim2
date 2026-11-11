using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string>? images)
        : base(Guid.NewGuid())
    {
        ValidateContent(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        Images = CopyTrimmed(images!);
    }

    public void Update(Guid userId, string name, string description, List<string>? images)
    {
        EnsureOwnedBy(userId);
        ValidateContent(name, description, images);

        Name = name;
        Description = description;
        Images = CopyTrimmed(images!);
    }

    public void EnsureOwnedBy(Guid userId)
    {
        if (OwnerId != userId)
        {
            throw new NotFoundException($"Club {Id} does not exist.");
        }
    }

    private static void ValidateContent(string name, string description, List<string>? images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images is null || images.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }
        if (images.Any(string.IsNullOrWhiteSpace))
        {
            throw new DomainException("An image address cannot be blank.");
        }
    }

    private static List<string> CopyTrimmed(List<string> images)
    {
        return images.Select(image => image.Trim()).ToList();
    }
}
