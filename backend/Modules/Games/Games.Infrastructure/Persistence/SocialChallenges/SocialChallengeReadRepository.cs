using Games.Application.Locations;
using Games.Application.SocialChallengeManagement;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The end of the window is computed in memory by TimeWindow, so the row is loaded first and turned into the DTO after.
    public async Task<SocialChallengeDto?> GetByIdAsync(Guid id)
    {
        var row = await _dbContext.SocialChallenges
            .Where(challenge => challenge.Id == id)
            .Select(challenge => new SocialChallengeRow
            {
                Id = challenge.Id,
                Name = challenge.Name,
                Start = challenge.Window.Start,
                Duration = challenge.Window.Duration,
                RequiredParticipants = challenge.RequiredParticipants,
                RangeInMeters = challenge.RangeInMeters,
                Xp = challenge.Xp,
                CompletionMessage = challenge.CompletionMessage,
                Status = challenge.Status,
                Location = challenge.Location == null
                    ? null
                    : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude)
            })
            .FirstOrDefaultAsync();

        return row is null ? null : ToDto(row);
    }

    private static SocialChallengeDto ToDto(SocialChallengeRow row)
    {
        return new SocialChallengeDto(
            row.Id,
            row.Name,
            row.Start,
            row.Duration,
            new TimeWindow(row.Start, row.Duration).End(),
            row.RequiredParticipants,
            row.RangeInMeters,
            row.Xp,
            row.CompletionMessage,
            row.Status,
            row.Location);
    }

    private sealed class SocialChallengeRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public TimeOnly Start { get; init; }
        public TimeSpan Duration { get; init; }
        public int RequiredParticipants { get; init; }
        public int RangeInMeters { get; init; }
        public int Xp { get; init; }
        public string CompletionMessage { get; init; } = null!;
        public SocialChallengeStatus Status { get; init; }
        public LocationDto? Location { get; init; }
    }
}
