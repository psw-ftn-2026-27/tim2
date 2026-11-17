using Positions.Domain.UserPositions;

namespace Positions.Application.UserPositions;

public interface IUserPositionRepository
{
    Task<UserPosition?> GetByUserIdAsync(Guid userId);

    void Add(UserPosition position);
}
