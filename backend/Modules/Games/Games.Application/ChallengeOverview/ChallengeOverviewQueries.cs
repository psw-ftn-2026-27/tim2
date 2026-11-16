using Games.Application.Challenges;
using Shared.Domain;

namespace Games.Application.ChallengeOverview;

public sealed class ChallengeOverviewQueries
{
    private readonly IChallengeReadRepository _challengeReadRepository;

    public ChallengeOverviewQueries(IChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public Task<PageResult<ChallengeDto>> GetAllAsync(ChallengeType? type, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _challengeReadRepository.GetAllAsync(type, page, pageSize);
    }
}
