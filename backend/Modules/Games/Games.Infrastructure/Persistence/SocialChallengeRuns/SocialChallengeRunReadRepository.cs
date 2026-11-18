using Games.Application.Locations;
using Games.Application.SocialChallengeRuns;
using Games.Application.SocialChallengeSolving;
using Games.Domain.SocialChallengeRuns;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallengeRuns;

internal sealed class SocialChallengeRunReadRepository : ISocialChallengeRunReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeRunReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Shows the run as the last check left it; the 20 s rule is not applied again here.
    // A solved run the player was in comes first, because a solved challenge cannot be activated again.
    // Otherwise the player's state comes from the latest run of the challenge.
    public async Task<SocialChallengeProgressDto?> GetProgressAsync(Guid challengeId, Guid playerId, DateTime now)
    {
        var challenge = await FindChallengeAsync(challengeId);
        if (challenge is null)
        {
            return null;
        }

        var solvedRun = await FindSolvedRunAsync(challengeId, playerId);
        if (solvedRun is not null)
        {
            return ToProgress(challengeId, challenge, solvedRun, SocialChallengeProgressState.Solved);
        }

        var latestRun = await FindLatestRunAsync(challengeId, playerId);
        if (latestRun?.PlayerLastCheckedAt is null)
        {
            return ToNotInRunProgress(challengeId, challenge);
        }

        // An active run whose end has passed is shown as expired before any check writes the Expired status.
        var isRunning = latestRun.Status == SocialChallengeRunStatus.Active && latestRun.EndsAt > now;
        var state = isRunning ? SocialChallengeProgressState.InRun : SocialChallengeProgressState.Expired;
        return ToProgress(challengeId, challenge, latestRun, state);
    }

    private async Task<ChallengeRow?> FindChallengeAsync(Guid challengeId)
    {
        return await _dbContext.SocialChallenges
            .AsNoTracking()
            .Where(challenge => challenge.Id == challengeId)
            .Select(challenge => new ChallengeRow
            {
                Name = challenge.Name,
                Location = challenge.Location == null
                    ? null
                    : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude),
                RangeInMeters = challenge.RangeInMeters,
                RequiredParticipants = challenge.RequiredParticipants,
                CompletionMessage = challenge.CompletionMessage
            })
            .FirstOrDefaultAsync();
    }

    private async Task<RunRow?> FindSolvedRunAsync(Guid challengeId, Guid playerId)
    {
        var runs = _dbContext.SocialChallengeRuns
            .AsNoTracking()
            .Where(run => run.ChallengeId == challengeId
                && run.Status == SocialChallengeRunStatus.Solved
                && run.Participants.Any(participant => participant.PlayerId == playerId))
            .OrderByDescending(run => run.SolvedAt);

        return await ProjectRuns(runs, playerId).FirstOrDefaultAsync();
    }

    private async Task<RunRow?> FindLatestRunAsync(Guid challengeId, Guid playerId)
    {
        var runs = _dbContext.SocialChallengeRuns
            .AsNoTracking()
            .Where(run => run.ChallengeId == challengeId)
            .OrderByDescending(run => run.StartedAt);

        return await ProjectRuns(runs, playerId).FirstOrDefaultAsync();
    }

    private static IQueryable<RunRow> ProjectRuns(IQueryable<SocialChallengeRun> runs, Guid playerId)
    {
        return runs.Select(run => new RunRow
        {
            Status = run.Status,
            EndsAt = run.EndsAt,
            ParticipantCount = run.Participants.Count,
            PlayerLastCheckedAt = run.Participants
                .Where(participant => participant.PlayerId == playerId)
                .Select(participant => (DateTime?)participant.LastCheckedAt)
                .FirstOrDefault(),
            SolvedAt = run.SolvedAt,
            EarnedXp = run.EarnedXp
        });
    }

    private static SocialChallengeProgressDto ToProgress(Guid challengeId, ChallengeRow challenge, RunRow run, SocialChallengeProgressState state)
    {
        var isSolved = state == SocialChallengeProgressState.Solved;
        return new SocialChallengeProgressDto(
            challengeId,
            challenge.Name,
            challenge.Location,
            challenge.RangeInMeters,
            challenge.RequiredParticipants,
            run.ParticipantCount,
            run.PlayerLastCheckedAt,
            state,
            isSolved ? challenge.CompletionMessage : null,
            isSolved ? run.EarnedXp : null,
            isSolved ? run.SolvedAt : null);
    }

    private static SocialChallengeProgressDto ToNotInRunProgress(Guid challengeId, ChallengeRow challenge)
    {
        return new SocialChallengeProgressDto(
            challengeId,
            challenge.Name,
            challenge.Location,
            challenge.RangeInMeters,
            challenge.RequiredParticipants,
            0,
            null,
            SocialChallengeProgressState.NotInRun,
            null,
            null,
            null);
    }

    private sealed class ChallengeRow
    {
        public string Name { get; init; } = null!;
        public LocationDto? Location { get; init; }
        public int RangeInMeters { get; init; }
        public int RequiredParticipants { get; init; }
        public string CompletionMessage { get; init; } = null!;
    }

    private sealed class RunRow
    {
        public SocialChallengeRunStatus Status { get; init; }
        public DateTime EndsAt { get; init; }
        public int ParticipantCount { get; init; }
        public DateTime? PlayerLastCheckedAt { get; init; }
        public DateTime? SolvedAt { get; init; }
        public int? EarnedXp { get; init; }
    }
}
