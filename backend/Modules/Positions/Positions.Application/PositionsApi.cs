using Positions.Application.UserPositions;
using Positions.Contracts;

namespace Positions.Application;

public sealed class PositionsApi : IPositionsApi
{
    private readonly IUserPositionReadRepository _positionReadRepository;

    public PositionsApi(IUserPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public async Task<PositionSummary?> GetPositionAsync(Guid userId)
    {
        var position = await _positionReadRepository.GetByUserIdAsync(userId);
        return position is null ? null : new PositionSummary(position.Latitude, position.Longitude);
    }
}
