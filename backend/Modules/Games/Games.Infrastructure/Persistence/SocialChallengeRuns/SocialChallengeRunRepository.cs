using Games.Application.SocialChallengeRuns;
using Games.Domain.SocialChallengeRuns;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallengeRuns;

internal sealed class SocialChallengeRunRepository : ISocialChallengeRunRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeRunRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallengeRun?> GetActiveByChallengeAsync(Guid challengeId)
    {
        return _dbContext.SocialChallengeRuns
            .Where(run => run.ChallengeId == challengeId && run.Status == SocialChallengeRunStatus.Active)
            .OrderBy(run => run.StartedAt)
            .FirstOrDefaultAsync();
    }

    public Task<SocialChallengeRun?> GetSolvedByChallengeAndPlayerAsync(Guid challengeId, Guid playerId)
    {
        return _dbContext.SocialChallengeRuns
            .Where(run => run.ChallengeId == challengeId
                && run.Status == SocialChallengeRunStatus.Solved
                && run.Participants.Any(participant => participant.PlayerId == playerId))
            .OrderByDescending(run => run.SolvedAt)
            .FirstOrDefaultAsync();
    }

    public void Add(SocialChallengeRun run)
    {
        _dbContext.SocialChallengeRuns.Add(run);
    }
}
