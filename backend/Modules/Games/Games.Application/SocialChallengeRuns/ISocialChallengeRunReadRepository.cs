using Games.Application.SocialChallengeSolving;

namespace Games.Application.SocialChallengeRuns;

public interface ISocialChallengeRunReadRepository
{
    Task<SocialChallengeProgressDto?> GetProgressAsync(Guid challengeId, Guid playerId, DateTime now);
}
