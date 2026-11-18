using Games.Application.LocationChallengeSolving;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptReadRepository
{
    Task<LocationChallengeProgressDto?> GetProgressAsync(Guid challengeId, Guid playerId);
}
