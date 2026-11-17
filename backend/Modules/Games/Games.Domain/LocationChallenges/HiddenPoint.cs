using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record HiddenPoint
{
    public string ImageUrl { get; private init; }
    public Location Location { get; private init; }

    private HiddenPoint()
    {
        ImageUrl = null!;
        Location = null!;
    }

    public HiddenPoint(string imageUrl, Location location)
    {
        var trimmedImageUrl = imageUrl?.Trim() ?? string.Empty;
        if (trimmedImageUrl.Length == 0)
        {
            throw new DomainException("A hidden point requires a photo.");
        }
        if (location is null)
        {
            throw new DomainException("A hidden point requires a location.");
        }

        ImageUrl = trimmedImageUrl;
        Location = location;
    }
}
