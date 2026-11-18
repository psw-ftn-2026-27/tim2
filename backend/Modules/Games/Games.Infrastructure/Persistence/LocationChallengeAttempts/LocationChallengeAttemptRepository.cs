using Games.Application.LocationChallengeAttempts;
using Games.Domain.LocationChallengeAttempts;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal sealed class LocationChallengeAttemptRepository : ILocationChallengeAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallengeAttempt?> GetByChallengePlayerAndRoundAsync(Guid challengeId, Guid playerId, int round)
    {
        return _dbContext.LocationChallengeAttempts.FirstOrDefaultAsync(attempt =>
            attempt.ChallengeId == challengeId && attempt.PlayerId == playerId && attempt.Round == round);
    }

    public Task<List<LocationChallengeAttempt>> GetAllByChallengeAsync(Guid challengeId)
    {
        return _dbContext.LocationChallengeAttempts
            .Where(attempt => attempt.ChallengeId == challengeId)
            .ToListAsync();
    }

    public void Add(LocationChallengeAttempt attempt)
    {
        _dbContext.LocationChallengeAttempts.Add(attempt);
    }

    public void RemoveRange(List<LocationChallengeAttempt> attempts)
    {
        _dbContext.LocationChallengeAttempts.RemoveRange(attempts);
    }
}
