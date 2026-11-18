using Games.Application.SocialChallengeRuns;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Positions.Contracts;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeSolving;

public sealed class SocialChallengeSolvingService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly ISocialChallengeRunRepository _runRepository;
    private readonly SocialChallengeSolver _challengeSolver;
    private readonly IPositionsApi _positionsApi;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeSolvingService(
        ISocialChallengeRepository challengeRepository,
        ISocialChallengeRunRepository runRepository,
        SocialChallengeSolver challengeSolver,
        IPositionsApi positionsApi,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _runRepository = runRepository;
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
        var activeRun = await _runRepository.GetActiveByChallengeAsync(challengeId);
        var solvedRun = await _runRepository.GetSolvedByChallengeAndPlayerAsync(challengeId, playerId);

        var newRun = _challengeSolver.Activate(challenge, activeRun, solvedRun, playerId, position.Latitude, position.Longitude, DateTime.UtcNow);

        if (newRun is not null)
        {
            _runRepository.Add(newRun);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    // A missing position does not reject the check: the solver removes the player as if out of range.
    // The player's solved run comes first, so a later run started by other players does not reject the check.
    public async Task CheckAsync(Guid challengeId, Guid playerId)
    {
        var position = await _positionsApi.GetPositionAsync(playerId);
        var challenge = await GetChallengeAsync(challengeId);
        var run = await _runRepository.GetSolvedByChallengeAndPlayerAsync(challengeId, playerId)
            ?? await _runRepository.GetActiveByChallengeAsync(challengeId);
        if (run is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} has no run you can check.");
        }

        _challengeSolver.Check(challenge, run, playerId, position?.Latitude, position?.Longitude, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }
        return challenge;
    }
}
