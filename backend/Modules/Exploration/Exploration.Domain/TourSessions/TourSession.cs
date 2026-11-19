using Exploration.Domain.Locations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourSessions;

public sealed class TourSession : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourSessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public Location StartPosition { get; private set; }
    public DateTime? AbandonedAt { get; private set; }

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
        if (Status != TourSessionStatus.Active)
        {
            throw new DomainException("Only an active tour can be left.");
        }

        Status = TourSessionStatus.Abandoned;
        AbandonedAt = DateTime.UtcNow;
    }
}
