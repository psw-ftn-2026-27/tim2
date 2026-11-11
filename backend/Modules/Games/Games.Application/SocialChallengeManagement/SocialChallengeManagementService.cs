using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeManagement;

public sealed class SocialChallengeManagementService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeManagementService(ISocialChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateSocialChallengeDto dto)
    {
        var challenge = new SocialChallenge(
            dto.Name,
            dto.Start,
            dto.Duration,
            dto.RequiredParticipants,
            dto.RangeInMeters,
            dto.Xp,
            dto.CompletionMessage);

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    public async Task UpdateAsync(Guid challengeId, UpdateSocialChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Update(
            dto.Name,
            dto.Start,
            dto.Duration,
            dto.RequiredParticipants,
            dto.RangeInMeters,
            dto.Xp,
            dto.CompletionMessage);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.EnsureCanBeDeleted();
        _challengeRepository.Remove(challenge);
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
