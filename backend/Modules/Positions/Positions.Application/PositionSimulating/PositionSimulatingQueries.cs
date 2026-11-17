using Positions.Application.UserPositions;

namespace Positions.Application.PositionSimulating;

public sealed class PositionSimulatingQueries
{
    private readonly IUserPositionReadRepository _positionReadRepository;

    public PositionSimulatingQueries(IUserPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public Task<UserPositionDto?> GetMyPositionAsync(Guid userId)
    {
        return _positionReadRepository.GetByUserIdAsync(userId);
    }
}
