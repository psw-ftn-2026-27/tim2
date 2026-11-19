using Games.Application.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeManagement;

public sealed class SocialChallengeManagementQueries
{
    private readonly ISocialChallengeReadRepository _challengeReadRepository;

    public SocialChallengeManagementQueries(ISocialChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public async Task<SocialChallengeDto> GetByIdAsync(Guid id)
    {
        var challenge = await _challengeReadRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {id} does not exist.");
        }

        return challenge;
    }
}
