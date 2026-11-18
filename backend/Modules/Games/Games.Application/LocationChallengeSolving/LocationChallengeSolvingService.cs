using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Positions.Contracts;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingService
{
    private const int MaximumExtraRadiusInMeters = 100;

    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly ILocationChallengeAttemptRepository _attemptRepository;
    private readonly LocationChallengeSolver _challengeSolver;
    private readonly IPositionsApi _positionsApi;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeSolvingService(
        ILocationChallengeRepository challengeRepository,
        ILocationChallengeAttemptRepository attemptRepository,
        LocationChallengeSolver challengeSolver,
        IPositionsApi positionsApi,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _attemptRepository = attemptRepository;
        _challengeSolver = challengeSolver;
        _positionsApi = positionsApi;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid challengeId, Guid playerId)
    {
        var position = await _positionsApi.GetPositionAsync(playerId);
        if (position is null)
        {
            throw new NotFoundException("Your position is not set. Click on the map to set it.");
        }
        var challenge = await GetChallengeAsync(challengeId);
        var currentAttempt = await _attemptRepository.GetByChallengePlayerAndRoundAsync(challengeId, playerId, challenge.CurrentRound);
        // Both bounds are included, so the upper bound passed to Next is one more than the maximum.
        var extraRadiusInMeters = Random.Shared.Next(0, MaximumExtraRadiusInMeters + 1);

        var attempt = _challengeSolver.Activate(
            challenge,
            currentAttempt,
            new Location(position.Latitude, position.Longitude),
            playerId,
            extraRadiusInMeters,
            DateTime.UtcNow);

        _attemptRepository.Add(attempt);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckAsync(Guid challengeId, Guid playerId)
    {
        var position = await _positionsApi.GetPositionAsync(playerId);
        if (position is null)
        {
            throw new NotFoundException("Your position is not set. Click on the map to set it.");
        }
        var challenge = await GetChallengeAsync(challengeId);
        var attempt = await _attemptRepository.GetByChallengePlayerAndRoundAsync(challengeId, playerId, challenge.CurrentRound);
        if (attempt is null)
        {
            throw new NotFoundException($"You have not activated location challenge {challengeId} in this round.");
        }

        _challengeSolver.Check(challenge, attempt, new Location(position.Latitude, position.Longitude), DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LocationChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {challengeId} does not exist.");
        }
        return challenge;
    }
}
