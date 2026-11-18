using Shared.Domain;

namespace Games.Domain.LocationChallengeAttempts;

public sealed class LocationChallengeAttempt : AggregateRoot
{
    public Guid ChallengeId { get; private set; }
    public Guid PlayerId { get; private set; }
    public int Round { get; private set; }
    public int SearchRadiusInMeters { get; private set; }
    public DateTime ActivatedAt { get; private set; }

    private LocationChallengeAttempt() { }

    public LocationChallengeAttempt(Guid challengeId, Guid playerId, int round, int searchRadiusInMeters, DateTime activatedAt)
        : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
        PlayerId = playerId;
        Round = round;
        SearchRadiusInMeters = searchRadiusInMeters;
        ActivatedAt = activatedAt;
    }
}
