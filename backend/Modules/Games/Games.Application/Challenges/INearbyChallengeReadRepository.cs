using Games.Application.NearbyChallenges;

namespace Games.Application.Challenges;

public interface INearbyChallengeReadRepository
{
    Task<List<NearbyChallengeDto>> GetNearbyAsync(Guid playerId, double latitude, double longitude);
}
