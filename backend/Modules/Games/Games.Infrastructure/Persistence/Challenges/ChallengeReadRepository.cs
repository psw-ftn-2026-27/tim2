using Games.Application.ChallengeOverview;
using Games.Application.Challenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.Challenges;

internal sealed class ChallengeReadRepository : IChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public ChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<ChallengeDto>> GetAllAsync(ChallengeType? type, int page, int pageSize)
    {
        var challenges = SelectRows(type)
            .OrderBy(row => row.Name)
            .ThenBy(row => row.Id);

        var totalCount = await challenges.CountAsync();
        var rows = await challenges
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows.Select(ToDto).ToList();
        return new PageResult<ChallengeDto>(items, totalCount);
    }

    // Both tables are projected to the same row shape, so the database pages and sorts them as one list.
    private IQueryable<ChallengeRow> SelectRows(ChallengeType? type)
    {
        return type switch
        {
            ChallengeType.SocialChallenge => SelectSocialChallenges(),
            ChallengeType.Quiz => SelectQuizzes(),
            _ => SelectSocialChallenges().Concat(SelectQuizzes())
        };
    }

    private IQueryable<ChallengeRow> SelectSocialChallenges()
    {
        return _dbContext.SocialChallenges.Select(challenge => new ChallengeRow
        {
            Id = challenge.Id,
            Type = ChallengeType.SocialChallenge,
            Name = challenge.Name,
            Xp = challenge.Xp,
            Status = challenge.Status.ToString(),
            Start = challenge.Window.Start,
            Duration = challenge.Window.Duration,
            RequiredParticipants = challenge.RequiredParticipants,
            RangeInMeters = challenge.RangeInMeters,
            CompletionMessage = challenge.CompletionMessage
        });
    }

    private IQueryable<ChallengeRow> SelectQuizzes()
    {
        return _dbContext.Quizzes.Select(quiz => new ChallengeRow
        {
            Id = quiz.Id,
            Type = ChallengeType.Quiz,
            Name = quiz.Name,
            Xp = quiz.Xp,
            Status = quiz.Status.ToString(),
            Start = null,
            Duration = null,
            RequiredParticipants = null,
            RangeInMeters = null,
            CompletionMessage = null
        });
    }

    private static ChallengeDto ToDto(ChallengeRow row)
    {
        return new ChallengeDto(
            row.Id,
            row.Type,
            row.Name,
            row.Xp,
            row.Status,
            row.Start,
            row.Duration,
            row.Start is null || row.Duration is null ? null : new TimeWindow(row.Start.Value, row.Duration.Value).End(),
            row.RequiredParticipants,
            row.RangeInMeters,
            row.CompletionMessage);
    }

    private sealed class ChallengeRow
    {
        public Guid Id { get; init; }
        public ChallengeType Type { get; init; }
        public string Name { get; init; } = null!;
        public int Xp { get; init; }
        public string Status { get; init; } = null!;
        public TimeOnly? Start { get; init; }
        public TimeSpan? Duration { get; init; }
        public int? RequiredParticipants { get; init; }
        public int? RangeInMeters { get; init; }
        public string? CompletionMessage { get; init; }
    }
}
