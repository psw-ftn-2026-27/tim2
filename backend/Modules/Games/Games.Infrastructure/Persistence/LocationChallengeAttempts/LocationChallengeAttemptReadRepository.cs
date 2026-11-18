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

        var searchRadiusInMeters = await FindSearchRadiusInCurrentRoundAsync(challengeId, playerId, challenge.CurrentRound);
        if (searchRadiusInMeters is null)
        {
            return EmptyProgress(challengeId, LocationChallengeProgressState.NotActivated);
        }

        return new LocationChallengeProgressDto(
            challengeId,
            LocationChallengeProgressState.Active,
            challenge.Name,
            new LocationDto(challenge.StartLatitude, challenge.StartLongitude),
            searchRadiusInMeters,
            ToHiddenPoints(challenge.ImageUrls));
    }

    // Only the photos of the hidden points are selected, never their locations, so a location cannot be sent by mistake.
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
                ImageUrls = challenge.HiddenPoints.Select(point => point.ImageUrl).ToList()
            })
            .FirstOrDefaultAsync();
    }

    private async Task<int?> FindSearchRadiusInCurrentRoundAsync(Guid challengeId, Guid playerId, int currentRound)
    {
        return await _dbContext.LocationChallengeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.ChallengeId == challengeId && attempt.PlayerId == playerId && attempt.Round == currentRound)
            .Select(attempt => (int?)attempt.SearchRadiusInMeters)
            .FirstOrDefaultAsync();
    }

    // No point can be discovered yet, so every location stays empty; checking a hidden point will add the discovered ones.
    private static List<HiddenPointProgressDto> ToHiddenPoints(List<string> imageUrls)
    {
        return imageUrls
            .Select((imageUrl, index) => new HiddenPointProgressDto(index + 1, imageUrl, null))
            .ToList();
    }

    private static LocationChallengeProgressDto EmptyProgress(Guid challengeId, LocationChallengeProgressState state)
    {
        return new LocationChallengeProgressDto(challengeId, state, null, null, null, null);
    }

    private sealed class ChallengeRow
    {
        public string Name { get; init; } = null!;
        public LocationChallengeStatus Status { get; init; }
        public int CurrentRound { get; init; }
        public double StartLatitude { get; init; }
        public double StartLongitude { get; init; }
        public List<string> ImageUrls { get; init; } = null!;
    }
}
