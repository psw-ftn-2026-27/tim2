using Games.Application.Challenges;
using Positions.Contracts;

namespace Games.Application.NearbyChallenges;

public sealed class NearbyChallengesQueries
{
    private readonly INearbyChallengeReadRepository _nearbyChallengeReadRepository;
    private readonly IPositionsApi _positionsApi;

    public NearbyChallengesQueries(INearbyChallengeReadRepository nearbyChallengeReadRepository, IPositionsApi positionsApi)
    {
        _nearbyChallengeReadRepository = nearbyChallengeReadRepository;
        _positionsApi = positionsApi;
    }

    public async Task<List<NearbyChallengeDto>> GetAllAsync(Guid playerId)
    {
        var position = await _positionsApi.GetPositionAsync(playerId);
        if (position is null)
        {
            return [];
        }

        return await _nearbyChallengeReadRepository.GetNearbyAsync(playerId, position.Latitude, position.Longitude);
    }
}
