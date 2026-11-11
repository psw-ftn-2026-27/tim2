using Games.Application.SocialChallenges;
using Shared.Domain;

namespace Games.Application.SocialChallengeManagement;

public sealed class SocialChallengeManagementQueries
{
    private readonly ISocialChallengeReadRepository _challengeReadRepository;

    public SocialChallengeManagementQueries(ISocialChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _challengeReadRepository.GetAllAsync(page, pageSize);
    }
}
