using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallengeSolving;
using Games.Application.Locations;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal sealed class LocationChallengeAttemptReadRepository : ILocationChallengeAttemptReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeAttemptReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The state is derived from the challenge state and round, without a domain check, so a retired
    // or repeated challenge is reported in the answer instead of as an error.
    public async Task<LocationChallengeProgressDto?> GetProgressAsync(Guid challengeId, Guid playerId)
    {
        var challenge = await FindChallengeAsync(challengeId);
        if (challenge is null)
        {
            return null;
        }
        if (challenge.Status != LocationChallengeStatus.Published)
        {
            return EmptyProgress(challengeId, LocationChallengeProgressState.Unavailable);
        }

        var attempt = await FindAttemptInCurrentRoundAsync(challengeId, playerId, challenge.CurrentRound);
        if (attempt is null)
        {
            return EmptyProgress(challengeId, LocationChallengeProgressState.NotActivated);
        }

        return new LocationChallengeProgressDto(
            challengeId,
            attempt.SolvedAt is null ? LocationChallengeProgressState.Active : LocationChallengeProgressState.Solved,
            challenge.Name,
            new LocationDto(challenge.StartLatitude, challenge.StartLongitude),
            attempt.SearchRadiusInMeters,
            ToHiddenPoints(challenge.HiddenPoints, attempt.DiscoveredPointNumbers),
            attempt.SolvedAt,
            attempt.EarnedXp);
    }

    private async Task<ChallengeRow?> FindChallengeAsync(Guid challengeId)
    {
        return await _dbContext.LocationChallenges
            .AsNoTracking()
            .Where(challenge => challenge.Id == challengeId)
            .Select(challenge => new ChallengeRow
            {
                Name = challenge.Name,
                Status = challenge.Status,
                CurrentRound = challenge.CurrentRound,
                StartLatitude = challenge.StartLocation.Latitude,
                StartLongitude = challenge.StartLocation.Longitude,
                HiddenPoints = challenge.HiddenPoints
                    .Select(point => new HiddenPointRow
                    {
                        ImageUrl = point.ImageUrl,
                        Latitude = point.Location.Latitude,
                        Longitude = point.Location.Longitude
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    private async Task<AttemptRow?> FindAttemptInCurrentRoundAsync(Guid challengeId, Guid playerId, int currentRound)
    {
        return await _dbContext.LocationChallengeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.ChallengeId == challengeId && attempt.PlayerId == playerId && attempt.Round == currentRound)
            .Select(attempt => new AttemptRow
            {
                SearchRadiusInMeters = attempt.SearchRadiusInMeters,
                DiscoveredPointNumbers = attempt.DiscoveredPointNumbers,
                SolvedAt = attempt.SolvedAt,
                EarnedXp = attempt.EarnedXp
            })
            .FirstOrDefaultAsync();
    }

    // Numbers count from 1. Only a discovered point gets its location, so an undiscovered location is never sent.
    private static List<HiddenPointProgressDto> ToHiddenPoints(List<HiddenPointRow> hiddenPoints, List<int> discoveredPointNumbers)
    {
        return hiddenPoints
            .Select((point, index) => new HiddenPointProgressDto(
                index + 1,
                point.ImageUrl,
                discoveredPointNumbers.Contains(index + 1) ? new LocationDto(point.Latitude, point.Longitude) : null))
            .ToList();
    }

    private static LocationChallengeProgressDto EmptyProgress(Guid challengeId, LocationChallengeProgressState state)
    {
        return new LocationChallengeProgressDto(challengeId, state, null, null, null, null, null, null);
    }

    private sealed class ChallengeRow
    {
        public string Name { get; init; } = null!;
        public LocationChallengeStatus Status { get; init; }
        public int CurrentRound { get; init; }
        public double StartLatitude { get; init; }
        public double StartLongitude { get; init; }
        public List<HiddenPointRow> HiddenPoints { get; init; } = null!;
    }

    private sealed class HiddenPointRow
    {
        public string ImageUrl { get; init; } = null!;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    private sealed class AttemptRow
    {
        public int SearchRadiusInMeters { get; init; }
        public List<int> DiscoveredPointNumbers { get; init; } = null!;
        public DateTime? SolvedAt { get; init; }
        public int? EarnedXp { get; init; }
    }
}
