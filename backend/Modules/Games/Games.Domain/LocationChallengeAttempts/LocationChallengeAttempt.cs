using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

public sealed class LocationChallengeAttempt : AggregateRoot
{
    public Guid ChallengeId { get; private set; }
    public Guid PlayerId { get; private set; }
    public int Round { get; private set; }
    public int SearchRadiusInMeters { get; private set; }
    public DateTime ActivatedAt { get; private set; }
    // Ordinal numbers of the discovered hidden points, counted from 1 like the points of the challenge.
    public List<int> DiscoveredPointNumbers { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? EarnedXp { get; private set; }

    private LocationChallengeAttempt()
    {
        DiscoveredPointNumbers = null!;
    }

    public LocationChallengeAttempt(Guid challengeId, Guid playerId, int round, int searchRadiusInMeters, DateTime activatedAt)
        : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
        PlayerId = playerId;
        Round = round;
        SearchRadiusInMeters = searchRadiusInMeters;
        ActivatedAt = activatedAt;
        DiscoveredPointNumbers = [];
    }

    // A check without points in range is an ordinary miss, not an error, so an empty list leaves the attempt unchanged.
    public void RecordDiscovery(List<int> numbersInRange, int pointCount, int xp, DateTime checkedAt)
    {
        if (SolvedAt is not null)
        {
            throw new DomainException("You have already solved this location challenge, so it does not accept new checks.");
        }

        var newNumbers = numbersInRange
            .Where(number => !DiscoveredPointNumbers.Contains(number))
            .Distinct()
            .ToList();
        DiscoveredPointNumbers.AddRange(newNumbers);

        if (DiscoveredPointNumbers.Count == pointCount)
        {
            SolvedAt = checkedAt;
            EarnedXp = xp;
        }
    }
}
