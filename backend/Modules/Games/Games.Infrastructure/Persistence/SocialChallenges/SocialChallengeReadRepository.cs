using Games.Application.SocialChallengeManagement;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        var challenges = _dbContext.SocialChallenges
            .OrderBy(challenge => challenge.Name)
            .ThenBy(challenge => challenge.Id);

        var totalCount = await challenges.CountAsync();
        var items = await challenges
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(challenge => new SocialChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Window.Start,
                challenge.Window.Duration,
                new TimeWindow(challenge.Window.Start, challenge.Window.Duration).End(),
                challenge.RequiredParticipants,
                challenge.RangeInMeters,
                challenge.Xp,
                challenge.CompletionMessage,
                challenge.Status))
            .ToListAsync();

        return new PageResult<SocialChallengeDto>(items, totalCount);
    }
}
