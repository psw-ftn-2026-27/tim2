using Games.Application.ChallengeOverview;
using Games.Application.NearbyChallenges;
using Shared.Domain;

namespace Games.Application.Challenges;

public interface IChallengeReadRepository
{
    Task<PageResult<ChallengeDto>> GetAllAsync(ChallengeType? type, int page, int pageSize);
    Task<List<NearbyChallengeDto>> GetNearbyAsync(Guid playerId, double latitude, double longitude);
}
