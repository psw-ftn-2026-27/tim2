using Games.Domain.LocationChallengeAttempts;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptRepository
{
    Task<LocationChallengeAttempt?> GetByChallengePlayerAndRoundAsync(Guid challengeId, Guid playerId, int round);
    Task<List<LocationChallengeAttempt>> GetAllByChallengeAsync(Guid challengeId);
    void Add(LocationChallengeAttempt attempt);
    void RemoveRange(List<LocationChallengeAttempt> attempts);
}
