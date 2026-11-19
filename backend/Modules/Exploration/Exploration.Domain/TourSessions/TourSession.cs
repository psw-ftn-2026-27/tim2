using Exploration.Domain.Locations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourSessions;

public sealed class TourSession : AggregateRoot
{
    private readonly List<KeyPointCompletion> _completedKeyPoints = [];

    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourSessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public Location StartPosition { get; private set; }
    public DateTime? AbandonedAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyList<KeyPointCompletion> CompletedKeyPoints => _completedKeyPoints;

    private TourSession()
    {
        StartPosition = null!;
    }

    public TourSession(Guid touristId, Guid tourId, Location startPosition)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        TourId = tourId;
        Status = TourSessionStatus.Active;
        StartedAt = DateTime.UtcNow;
        StartPosition = startPosition;
    }

    public void Abandon()
    {
        EnsureActive("Only an active tour can be left.");

        Status = TourSessionStatus.Abandoned;
        AbandonedAt = DateTime.UtcNow;
    }

    public void RecordActivity()
    {
        EnsureActive("Arrival can be checked only on an active tour.");

        LastActivityAt = DateTime.UtcNow;
    }

    public void CompleteKeyPoint(Guid keyPointId, bool isLast)
    {
        EnsureActive("A key point can be reached only on an active tour.");

        var now = DateTime.UtcNow;
        _completedKeyPoints.Add(new KeyPointCompletion(keyPointId, now));

        if (isLast)
        {
            Status = TourSessionStatus.Completed;
            CompletedAt = now;
        }
    }

    private void EnsureActive(string message)
    {
        if (Status != TourSessionStatus.Active)
        {
            throw new DomainException(message);
        }
    }
}
