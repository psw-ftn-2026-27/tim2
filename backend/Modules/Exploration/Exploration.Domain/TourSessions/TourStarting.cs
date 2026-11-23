using Exploration.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourSessions;

public sealed class TourStarting
{
    // Returns the session the tourist continues in: the active one for the same tour, or a new one.
    // Only an active session resumes or blocks the start; a completed or abandoned one is ignored.
    public TourSession Start(Guid touristId, Guid tourId, bool isPurchased, Location? position, TourSession? activeSession)
    {
        if (!isPurchased)
        {
            throw new DomainException("You can start only a tour you have purchased.");
        }
        if (position is null)
        {
            throw new DomainException("Your position is not set. Set it before you start the tour.");
        }

        if (activeSession is { Status: TourSessionStatus.Active })
        {
            if (activeSession.TourId == tourId)
            {
                return activeSession;
            }

            throw new DomainException("Another tour is active. Leave it before you start this one.");
        }

        return new TourSession(touristId, tourId, position);
    }
}
