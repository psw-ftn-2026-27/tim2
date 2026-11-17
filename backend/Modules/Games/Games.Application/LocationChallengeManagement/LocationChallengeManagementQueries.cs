using Games.Application.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeManagement;

public sealed class LocationChallengeManagementQueries
{
    private readonly ILocationChallengeReadRepository _challengeReadRepository;

    public LocationChallengeManagementQueries(ILocationChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public async Task<LocationChallengeDto> GetByIdAsync(Guid id)
    {
        var challenge = await _challengeReadRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {id} does not exist.");
        }

        return challenge;
    }
}
