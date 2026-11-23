using Positions.Application.UserPositions;
using Positions.Domain.UserPositions;

namespace Positions.Application.PositionSimulating;

public sealed class PositionSimulatingService
{
    private readonly IUserPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PositionSimulatingService(IUserPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SetMyPositionAsync(Guid userId, UserPositionDto dto)
    {
        var coordinates = new Coordinates(dto.Latitude, dto.Longitude);

        var position = await _positionRepository.GetByUserIdAsync(userId);
        if (position is null)
        {
            _positionRepository.Add(new UserPosition(userId, coordinates));
        }
        else
        {
            position.MoveTo(coordinates);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
