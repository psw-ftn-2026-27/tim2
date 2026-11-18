using Games.Application.SocialChallengeRuns;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeSolving;

public sealed class SocialChallengeSolvingQueries
{
    private readonly ISocialChallengeRunReadRepository _runReadRepository;

    public SocialChallengeSolvingQueries(ISocialChallengeRunReadRepository runReadRepository)
    {
        _runReadRepository = runReadRepository;
    }

    public async Task<SocialChallengeProgressDto> GetProgressAsync(Guid challengeId, Guid playerId)
    {
        var progress = await _runReadRepository.GetProgressAsync(challengeId, playerId, DateTime.UtcNow);
        if (progress is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }

        return progress;
    }
}
