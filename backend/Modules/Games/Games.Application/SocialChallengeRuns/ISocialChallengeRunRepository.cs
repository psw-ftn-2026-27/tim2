using Games.Domain.SocialChallengeRuns;

namespace Games.Application.SocialChallengeRuns;

public interface ISocialChallengeRunRepository
{
    Task<SocialChallengeRun?> GetActiveByChallengeAsync(Guid challengeId);
    Task<SocialChallengeRun?> GetSolvedByChallengeAndPlayerAsync(Guid challengeId, Guid playerId);
    void Add(SocialChallengeRun run);
}
