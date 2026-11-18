using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

public sealed class LocationChallengeSolver
{
    // The extra radius is chosen by the caller, so the activation stays deterministic and testable.
    public LocationChallengeAttempt Activate(
        LocationChallenge challenge,
        LocationChallengeAttempt? currentAttempt,
        Location position,
        Guid playerId,
        int extraRadiusInMeters,
        DateTime now)
    {
        challenge.EnsureCanBePlayed();
        if (currentAttempt is not null && currentAttempt.Round == challenge.CurrentRound)
        {
            throw new DomainException("You have already activated this location challenge in this round.");
        }
        if (!challenge.IsWithinActivationRange(position))
        {
            throw new DomainException($"You are outside the activation range. Come within {LocationChallenge.ActivationRangeInMeters} m of the start location to activate it.");
        }

        return new LocationChallengeAttempt(
            challenge.Id,
            playerId,
            challenge.CurrentRound,
            challenge.SearchRadiusInMeters(extraRadiusInMeters),
            now);
    }

    // The challenge answers which points are in range, and the attempt decides which of them are new and when it is solved.
    public void Check(LocationChallenge challenge, LocationChallengeAttempt attempt, Location position, DateTime now)
    {
        challenge.EnsureCanBePlayed();
        if (attempt.Round != challenge.CurrentRound)
        {
            throw new DomainException("This attempt belongs to an earlier round of the location challenge, so it cannot be checked.");
        }

        var numbersInRange = challenge.PointNumbersWithinDiscoveryRange(position);
        attempt.RecordDiscovery(numbersInRange, challenge.HiddenPoints.Count, challenge.Xp, now);
    }
}
