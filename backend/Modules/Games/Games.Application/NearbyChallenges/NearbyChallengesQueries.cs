using Games.Application.Challenges;
using Positions.Contracts;

namespace Games.Application.NearbyChallenges;

public sealed class NearbyChallengesQueries
{
    private readonly IChallengeReadRepository _challengeReadRepository;
    private readonly IPositionsApi _positionsApi;

    public NearbyChallengesQueries(IChallengeReadRepository challengeReadRepository, IPositionsApi positionsApi)
    {
        _challengeReadRepository = challengeReadRepository;
        _positionsApi = positionsApi;
    }

    public async Task<List<NearbyChallengeDto>> GetAllAsync(Guid playerId)
    {
        var position = await _positionsApi.GetPositionAsync(playerId);
        if (position is null)
        {
            return [];
        }

        return await _challengeReadRepository.GetNearbyAsync(playerId, position.Latitude, position.Longitude);
    }
}
