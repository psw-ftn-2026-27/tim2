using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallengeDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.LocationChallenges
            .Where(challenge => challenge.Id == id)
            .Select(challenge => new LocationChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Xp,
                challenge.Status,
                new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude),
                challenge.HiddenPoints
                    .Select(point => new HiddenPointDto(
                        point.ImageUrl,
                        new LocationDto(point.Location.Latitude, point.Location.Longitude)))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
