using Games.Domain.LocationChallengeAttempts;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptRepository
{
    Task<LocationChallengeAttempt?> GetByChallengePlayerAndRoundAsync(Guid challengeId, Guid playerId, int round);
    void Add(LocationChallengeAttempt attempt);
}
