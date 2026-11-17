using Positions.Application.PositionSimulating;

namespace Positions.Application.UserPositions;

public interface IUserPositionReadRepository
{
    Task<UserPositionDto?> GetByUserIdAsync(Guid userId);
}
