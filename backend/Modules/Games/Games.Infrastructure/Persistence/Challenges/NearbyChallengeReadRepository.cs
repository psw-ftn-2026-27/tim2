using Games.Application.Challenges;
using Games.Application.NearbyChallenges;
using Games.Domain.LocationChallenges;
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

    // Quizzes, social challenges and location challenges have a location a player can walk to, so the nearby list holds all three.
    // The distance is computed in memory by Location, so the list is sorted after it is loaded.
    public async Task<List<NearbyChallengeDto>> GetNearbyAsync(Guid playerId, double latitude, double longitude, DateTime now)
    {
        var quizRows = await FindQuizRowsAsync(playerId);
        var socialChallenges = await LoadSocialChallengesAsync(playerId, now);
        var locationChallengeRows = await FindLocationChallengeRowsAsync(playerId);

        return quizRows
            .Concat(locationChallengeRows)
            .Select(row => ToDto(row, latitude, longitude))
            .Concat(socialChallenges.Select(row => ToDto(row, latitude, longitude, now)))
            .OrderBy(challenge => challenge.DistanceInMeters)
            .ThenBy(challenge => challenge.Id)
            .ToList();
    }

    private async Task<List<NearbyRow>> FindQuizRowsAsync(Guid playerId)
    {
        return await _dbContext.Quizzes
            .Where(quiz => quiz.Status == QuizStatus.Published && quiz.Location != null)
            .Select(quiz => new NearbyRow
            {
                Id = quiz.Id,
                Type = ChallengeType.Quiz,
                Name = quiz.Name,
                Xp = quiz.Xp,
                Latitude = quiz.Location!.Location.Latitude,
                Longitude = quiz.Location.Location.Longitude,
                RangeInMeters = quiz.Location.RangeInMeters,
                IsStarted = _dbContext.QuizAttempts
                    .Any(attempt => attempt.QuizId == quiz.Id && attempt.PlayerId == playerId),
                // A player has at most one attempt per quiz, and its EarnedXp is set when the last question is answered.
                EarnedXp = _dbContext.QuizAttempts
                    .Where(attempt => attempt.QuizId == quiz.Id && attempt.PlayerId == playerId)
                    .Select(attempt => attempt.EarnedXp)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    // Three short queries instead of one with two subqueries: the challenges, then the player's part in their runs.
    private async Task<List<NearbySocialChallengeRow>> LoadSocialChallengesAsync(Guid playerId, DateTime now)
    {
        var challenges = await LoadPublishedSocialChallengesAsync();
        var inActiveRun = await LoadChallengeIdsInActiveRunAsync(playerId, now);
        var earnedXp = await LoadEarnedXpByChallengeAsync(playerId);

        foreach (var challenge in challenges)
        {
            challenge.IsInActiveRun = inActiveRun.Contains(challenge.Id);
            challenge.EarnedXp = earnedXp.GetValueOrDefault(challenge.Id);
        }
        return challenges;
    }

    private async Task<List<NearbySocialChallengeRow>> LoadPublishedSocialChallengesAsync()
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
                WindowDuration = challenge.Window.Duration
            })
            .ToListAsync();
    }

    private async Task<HashSet<Guid>> LoadChallengeIdsInActiveRunAsync(Guid playerId, DateTime now)
    {
        var challengeIds = await _dbContext.SocialChallengeRuns
            .Where(run => run.Status == SocialChallengeRunStatus.Active
                && run.EndsAt > now
                && run.Participants.Any(participant => participant.PlayerId == playerId))
            .Select(run => run.ChallengeId)
            .ToListAsync();
        return challengeIds.ToHashSet();
    }

    // A challenge solved more than once (after it was repeated) shows the XP of the latest solution.
    private async Task<Dictionary<Guid, int?>> LoadEarnedXpByChallengeAsync(Guid playerId)
    {
        var solvedRuns = await _dbContext.SocialChallengeRuns
            .Where(run => run.Status == SocialChallengeRunStatus.Solved
                && run.Participants.Any(participant => participant.PlayerId == playerId))
            .Select(run => new SolvedRunRow { ChallengeId = run.ChallengeId, SolvedAt = run.SolvedAt, EarnedXp = run.EarnedXp })
            .ToListAsync();
        return solvedRuns
            .GroupBy(run => run.ChallengeId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(run => run.SolvedAt).First().EarnedXp);
    }

    // Only an attempt from the current round counts, so an attempt solved in an earlier round neither marks the row
    // as solved nor brings its earned XP.
    private async Task<List<NearbyRow>> FindLocationChallengeRowsAsync(Guid playerId)
    {
        return await _dbContext.LocationChallenges
            .Where(challenge => challenge.Status == LocationChallengeStatus.Published)
            .Select(challenge => new NearbyRow
            {
                Id = challenge.Id,
                Type = ChallengeType.LocationChallenge,
                Name = challenge.Name,
                Xp = challenge.Xp,
                Latitude = challenge.StartLocation.Latitude,
                Longitude = challenge.StartLocation.Longitude,
                RangeInMeters = LocationChallenge.ActivationRangeInMeters,
                IsStarted = _dbContext.LocationChallengeAttempts
                    .Any(attempt => attempt.ChallengeId == challenge.Id
                        && attempt.PlayerId == playerId
                        && attempt.Round == challenge.CurrentRound),
                EarnedXp = _dbContext.LocationChallengeAttempts
                    .Where(attempt => attempt.ChallengeId == challenge.Id
                        && attempt.PlayerId == playerId
                        && attempt.Round == challenge.CurrentRound)
                    .Select(attempt => attempt.EarnedXp)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    private static NearbyChallengeDto ToDto(NearbyRow row, double latitude, double longitude)
    {
        var distance = DistanceInMeters(row.Latitude, row.Longitude, latitude, longitude);
        return new NearbyChallengeDto(
            row.Id,
            row.Type,
            row.Name,
            row.Xp,
            row.Latitude,
            row.Longitude,
            distance,
            MetersToRange(distance, row.RangeInMeters),
            row.IsStarted,
            row.EarnedXp is not null,
            row.EarnedXp,
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
            row.EarnedXp,
            new NearbySocialChallengeDto(
                row.RequiredParticipants,
                row.WindowStart,
                new TimeWindow(row.WindowStart, row.WindowDuration).Contains(now)));
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

    private sealed class NearbyRow
    {
        public Guid Id { get; init; }
        public ChallengeType Type { get; init; }
        public string Name { get; init; } = null!;
        public int Xp { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public int RangeInMeters { get; init; }
        public bool IsStarted { get; init; }
        public int? EarnedXp { get; init; }
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
        // Filled after loading, from the player's runs.
        public bool IsInActiveRun { get; set; }
        public int? EarnedXp { get; set; }
    }

    private sealed class SolvedRunRow
    {
        public Guid ChallengeId { get; init; }
        public DateTime? SolvedAt { get; init; }
        public int? EarnedXp { get; init; }
    }
}
