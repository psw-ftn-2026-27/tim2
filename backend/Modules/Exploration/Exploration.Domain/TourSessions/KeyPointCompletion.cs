namespace Exploration.Domain.TourSessions;

// Points at the key point by ID, because the key point belongs to the Tour aggregate.
public sealed record KeyPointCompletion
{
    public Guid KeyPointId { get; private init; }
    public DateTime CompletedAt { get; private init; }

    private KeyPointCompletion() { }

    public KeyPointCompletion(Guid keyPointId, DateTime completedAt)
    {
        KeyPointId = keyPointId;
        CompletedAt = completedAt;
    }
}
