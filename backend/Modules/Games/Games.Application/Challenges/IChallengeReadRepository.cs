using Games.Application.ChallengeOverview;
using Shared.Domain;

namespace Games.Application.Challenges;

public interface IChallengeReadRepository
{
    Task<PageResult<ChallengeDto>> GetAllAsync(ChallengeType? type, int page, int pageSize);
}
