using Exploration.Domain.Locations;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourSessions;

public sealed class KeyPointArrival
{
    private const double ArrivalRangeInMeters = 50;

    // Every check is recorded as activity. Only the next uncompleted key point in the author's order
    // can be reached (P1); being near any other key point changes nothing.
    public void Check(TourSession session, Tour tour, Location? position)
    {
        if (position is null)
        {
            throw new DomainException("Your position is not set. Set it before you check your arrival.");
        }

        session.RecordActivity();

        var remainingKeyPoints = tour.KeyPoints
            .OrderBy(keyPoint => keyPoint.Order)
            .Where(keyPoint => session.CompletedKeyPoints.All(completion => completion.KeyPointId != keyPoint.Id))
            .ToList();
        if (remainingKeyPoints.Count == 0)
        {
            return;
        }

        var nextKeyPoint = remainingKeyPoints[0];
        if (nextKeyPoint.Location.DistanceInMetersTo(position) <= ArrivalRangeInMeters)
        {
            session.CompleteKeyPoint(nextKeyPoint.Id, remainingKeyPoints.Count == 1);
        }
    }
}
