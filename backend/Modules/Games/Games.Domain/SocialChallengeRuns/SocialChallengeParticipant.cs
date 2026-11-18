using Shared.Domain;

namespace Games.Domain.SocialChallengeRuns;

public sealed class SocialChallengeParticipant : Entity
{
    public Guid PlayerId { get; private set; }
    public DateTime LastCheckedAt { get; private set; }

    private SocialChallengeParticipant() { }

    internal SocialChallengeParticipant(Guid playerId, DateTime joinedAt)
        : base(Guid.NewGuid())
    {
        PlayerId = playerId;
        LastCheckedAt = joinedAt;
    }

    internal void RecordCheck(DateTime checkedAt)
    {
        LastCheckedAt = checkedAt;
    }
}
