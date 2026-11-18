using Games.Application.LocationChallengeAttempts;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingQueries
{
    private readonly ILocationChallengeAttemptReadRepository _attemptReadRepository;

    public LocationChallengeSolvingQueries(ILocationChallengeAttemptReadRepository attemptReadRepository)
    {
        _attemptReadRepository = attemptReadRepository;
    }

    public async Task<LocationChallengeProgressDto> GetProgressAsync(Guid challengeId, Guid playerId)
    {
        var progress = await _attemptReadRepository.GetProgressAsync(challengeId, playerId);
        if (progress is null)
        {
            throw new NotFoundException($"Location challenge {challengeId} does not exist.");
        }

        return progress;
    }
}
