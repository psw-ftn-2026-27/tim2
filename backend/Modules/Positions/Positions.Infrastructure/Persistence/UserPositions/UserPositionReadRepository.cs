using Microsoft.EntityFrameworkCore;
using Positions.Application.UserPositions;

namespace Positions.Infrastructure.Persistence.UserPositions;

internal sealed class UserPositionReadRepository : IUserPositionReadRepository
{
    private readonly PositionsDbContext _dbContext;

    public UserPositionReadRepository(PositionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserPositionDto?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserPositions
            .Where(position => position.UserId == userId)
            .Select(position => new UserPositionDto(
                position.Coordinates.Latitude,
                position.Coordinates.Longitude))
            .FirstOrDefaultAsync();
    }
}
