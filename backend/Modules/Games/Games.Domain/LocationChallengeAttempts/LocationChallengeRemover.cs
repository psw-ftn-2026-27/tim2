using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

public sealed class LocationChallengeRemover
{
    // The attempts come from every round, so a challenge solved in an earlier round also cannot be deleted.
    public void EnsureCanBeDeleted(LocationChallenge challenge, List<LocationChallengeAttempt> attempts)
    {
        challenge.EnsureCanBeDeleted();
        if (attempts.Any(attempt => attempt.IsSolved()))
        {
            throw new DomainException("This location challenge has been solved, so it cannot be deleted.");
        }
    }
}
