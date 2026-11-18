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
}
