using Games.Application.ChallengeOverview;
using Games.Application.Challenges;
using Games.Application.Locations;
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

    // All tables are projected to the same row shape, so the database pages and sorts them as one list.
    private IQueryable<ChallengeRow> SelectRows(ChallengeType? type)
    {
        return type switch
        {
            ChallengeType.SocialChallenge => SelectSocialChallenges(),
            ChallengeType.Quiz => SelectQuizzes(),
            ChallengeType.LocationChallenge => SelectLocationChallenges(),
            _ => SelectSocialChallenges().Concat(SelectQuizzes()).Concat(SelectLocationChallenges())
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
            Latitude = challenge.Location == null ? null : challenge.Location.Latitude,
            Longitude = challenge.Location == null ? null : challenge.Location.Longitude
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
            Latitude = quiz.Location == null ? null : quiz.Location.Location.Latitude,
            Longitude = quiz.Location == null ? null : quiz.Location.Location.Longitude
        });
    }

    private IQueryable<ChallengeRow> SelectLocationChallenges()
    {
        return _dbContext.LocationChallenges.Select(challenge => new ChallengeRow
        {
            Id = challenge.Id,
            Type = ChallengeType.LocationChallenge,
            Name = challenge.Name,
            Xp = challenge.Xp,
            Status = challenge.Status.ToString(),
            Latitude = challenge.StartLocation.Latitude,
            Longitude = challenge.StartLocation.Longitude
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
            row.Latitude is null || row.Longitude is null ? null : new LocationDto(row.Latitude.Value, row.Longitude.Value));
    }

    private sealed class ChallengeRow
    {
        public Guid Id { get; init; }
        public ChallengeType Type { get; init; }
        public string Name { get; init; } = null!;
        public int Xp { get; init; }
        public string Status { get; init; } = null!;
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
    }
}
