using Games.Application.Challenges;
using Games.Application.NearbyChallenges;
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

    // Only quizzes have a location a player can walk to, so for now the nearby list holds quizzes only.
    // The distance is computed in memory by Location, so the list is sorted after it is loaded.
    public async Task<List<NearbyChallengeDto>> GetNearbyAsync(Guid playerId, double latitude, double longitude)
    {
        var rows = await _dbContext.Quizzes
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

        return rows
            .Select(row => ToDto(row, latitude, longitude))
            .OrderBy(challenge => challenge.DistanceInMeters)
            .ThenBy(challenge => challenge.Id)
            .ToList();
    }

    private static NearbyChallengeDto ToDto(NearbyQuizRow row, double latitude, double longitude)
    {
        var distance = new Location(row.Latitude, row.Longitude).DistanceInMetersTo(new Location(latitude, longitude));
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
            row.IsSolved);
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
}
