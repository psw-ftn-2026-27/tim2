using Games.Application.Challenges;
using Games.Application.NearbyChallenges;
using Games.Domain.Locations;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Challenges;

internal sealed class NearbyChallengeReadRepository : INearbyChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public NearbyChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Quizzes and social challenges have a location a player can walk to, so the nearby list holds those two.
    // The distance is computed in memory by Location, so the list is sorted after it is loaded.
    public async Task<List<NearbyChallengeDto>> GetNearbyAsync(Guid playerId, double latitude, double longitude, DateTime now)
    {
        var quizzes = await LoadQuizzesAsync(playerId);
        var socialChallenges = await LoadSocialChallengesAsync(playerId, now);

        return quizzes.Select(row => ToDto(row, latitude, longitude))
            .Concat(socialChallenges.Select(row => ToDto(row, latitude, longitude, now)))
            .OrderBy(challenge => challenge.DistanceInMeters)
            .ThenBy(challenge => challenge.Id)
            .ToList();
    }

    private async Task<List<NearbyQuizRow>> LoadQuizzesAsync(Guid playerId)
    {
        return await _dbContext.Quizzes
            .Where(quiz => quiz.Status == QuizStatus.Published && quiz.Location != null)
            .Select(quiz => new NearbyQuizRow
            {
                Id = quiz.Id,
                Name = quiz.Name,
                Xp = quiz.Xp,
                Latitude = quiz.Location!.Location.Latitude,
                Longitude = quiz.Location.Location.Longitude,
                RangeInMeters = quiz.Location.RangeInMeters,
                IsStarted = _dbContext.QuizAttempts
                    .Any(attempt => attempt.QuizId == quiz.Id && attempt.PlayerId == playerId),
                IsSolved = _dbContext.QuizAttempts
                    .Any(attempt => attempt.QuizId == quiz.Id && attempt.PlayerId == playerId && attempt.SolvedAt != null)
            })
            .ToListAsync();
    }

    private async Task<List<NearbySocialChallengeRow>> LoadSocialChallengesAsync(Guid playerId, DateTime now)
    {
        return await _dbContext.SocialChallenges
            .Where(challenge => challenge.Status == SocialChallengeStatus.Published && challenge.Location != null)
            .Select(challenge => new NearbySocialChallengeRow
            {
                Id = challenge.Id,
                Name = challenge.Name,
                Xp = challenge.Xp,
                Latitude = challenge.Location!.Latitude,
                Longitude = challenge.Location.Longitude,
                RangeInMeters = challenge.RangeInMeters,
                RequiredParticipants = challenge.RequiredParticipants,
                WindowStart = challenge.Window.Start,
                WindowDuration = challenge.Window.Duration,
                IsInActiveRun = _dbContext.SocialChallengeRuns
                    .Any(run => run.ChallengeId == challenge.Id
                        && run.Status == SocialChallengeRunStatus.Active
                        && run.EndsAt > now
                        && run.Participants.Any(participant => participant.PlayerId == playerId)),
                EarnedXp = _dbContext.SocialChallengeRuns
                    .Where(run => run.ChallengeId == challenge.Id
                        && run.Status == SocialChallengeRunStatus.Solved
                        && run.Participants.Any(participant => participant.PlayerId == playerId))
                    .OrderByDescending(run => run.SolvedAt)
                    .Select(run => run.EarnedXp)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    private static NearbyChallengeDto ToDto(NearbyQuizRow row, double latitude, double longitude)
    {
        var distance = DistanceInMeters(row.Latitude, row.Longitude, latitude, longitude);
        return new NearbyChallengeDto(
            row.Id,
            ChallengeType.Quiz,
            row.Name,
            row.Xp,
            row.Latitude,
            row.Longitude,
            distance,
            MetersToRange(distance, row.RangeInMeters),
            row.IsStarted,
            row.IsSolved,
            null);
    }

    // A social challenge counts as started while the player is in its active run; EarnedXp is set only by a solved run.
    private static NearbyChallengeDto ToDto(NearbySocialChallengeRow row, double latitude, double longitude, DateTime now)
    {
        var distance = DistanceInMeters(row.Latitude, row.Longitude, latitude, longitude);
        return new NearbyChallengeDto(
            row.Id,
            ChallengeType.SocialChallenge,
            row.Name,
            row.Xp,
            row.Latitude,
            row.Longitude,
            distance,
            MetersToRange(distance, row.RangeInMeters),
            row.IsInActiveRun,
            row.EarnedXp is not null,
            new NearbySocialChallengeDto(
                row.RequiredParticipants,
                row.WindowStart,
                new TimeWindow(row.WindowStart, row.WindowDuration).Contains(now),
                row.EarnedXp));
    }

    private static double DistanceInMeters(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
    {
        return new Location(fromLatitude, fromLongitude).DistanceInMetersTo(new Location(toLatitude, toLongitude));
    }

    // 0 when the player is already inside the range; otherwise rounded up, so the player is never told a distance shorter than the real one.
    private static int MetersToRange(double distanceInMeters, int rangeInMeters)
    {
        return (int)Math.Ceiling(Math.Max(0, distanceInMeters - rangeInMeters));
    }

    private sealed class NearbyQuizRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public int Xp { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public int RangeInMeters { get; init; }
        public bool IsStarted { get; init; }
        public bool IsSolved { get; init; }
    }

    private sealed class NearbySocialChallengeRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public int Xp { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public int RangeInMeters { get; init; }
        public int RequiredParticipants { get; init; }
        public TimeOnly WindowStart { get; init; }
        public TimeSpan WindowDuration { get; init; }
        public bool IsInActiveRun { get; init; }
        public int? EarnedXp { get; init; }
    }
}
