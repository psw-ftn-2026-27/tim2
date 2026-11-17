using Games.Application.LocationChallengeManagement;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<LocationChallengeDto?> GetByIdAsync(Guid id);
}
