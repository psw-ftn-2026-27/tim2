using Games.Application.SocialChallengeManagement;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<SocialChallengeDto?> GetByIdAsync(Guid id);
}
