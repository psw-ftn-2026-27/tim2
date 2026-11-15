using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeLifecycle;

public sealed class SocialChallengeLifecycleService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeLifecycleService(ISocialChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RepeatAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Repeat();
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
