using Microsoft.EntityFrameworkCore;
using Positions.Application.UserPositions;
using Positions.Domain.UserPositions;

namespace Positions.Infrastructure.Persistence.UserPositions;

internal sealed class UserPositionRepository : IUserPositionRepository
{
    private readonly PositionsDbContext _dbContext;

    public UserPositionRepository(PositionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserPosition?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserPositions.FirstOrDefaultAsync(position => position.UserId == userId);
    }

    public void Add(UserPosition position)
    {
        _dbContext.UserPositions.Add(position);
    }
}
