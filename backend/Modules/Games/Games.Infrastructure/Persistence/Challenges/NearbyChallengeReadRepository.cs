using Games.Application.Challenges;
using Games.Application.NearbyChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Challenges;

internal sealed class NearbyChallengeReadRepository : INearbyChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public NearbyChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Quizzes and location challenges have a location a player can walk to, so the nearby list holds both.
    // The distance is computed in memory by Location, so the list is sorted after it is loaded.
    public async Task<List<NearbyChallengeDto>> GetNearbyAsync(Guid playerId, double latitude, double longitude)
    {
        var quizRows = await FindQuizRowsAsync(playerId);
        var locationChallengeRows = await FindLocationChallengeRowsAsync(playerId);

        return quizRows
            .Concat(locationChallengeRows)
            .Select(row => ToDto(row, latitude, longitude))
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
                IsSolved = _dbContext.QuizAttempts
                    .Any(attempt => attempt.QuizId == quiz.Id && attempt.PlayerId == playerId && attempt.SolvedAt != null),
                EarnedXp = _dbContext.QuizAttempts
                    .Where(attempt => attempt.QuizId == quiz.Id && attempt.PlayerId == playerId)
                    .Select(attempt => attempt.EarnedXp)
                    .FirstOrDefault()
            })
            .ToListAsync();
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
                IsSolved = _dbContext.LocationChallengeAttempts
                    .Any(attempt => attempt.ChallengeId == challenge.Id
                        && attempt.PlayerId == playerId
                        && attempt.Round == challenge.CurrentRound
                        && attempt.SolvedAt != null),
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
        var distance = new Location(row.Latitude, row.Longitude).DistanceInMetersTo(new Location(latitude, longitude));
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
            row.IsSolved,
            row.EarnedXp);
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
        public bool IsSolved { get; init; }
        public int? EarnedXp { get; init; }
    }
}
