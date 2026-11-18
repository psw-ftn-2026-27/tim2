using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallengeRuns;

public sealed class SocialChallengeRun : AggregateRoot
{
    private static readonly TimeSpan MaximumTimeWithoutCheck = TimeSpan.FromSeconds(20);

    private readonly List<SocialChallengeParticipant> _participants = [];

    public Guid ChallengeId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public SocialChallengeRunStatus Status { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? EarnedXp { get; private set; }
    public IReadOnlyList<SocialChallengeParticipant> Participants => _participants;

    private SocialChallengeRun() { }

    // The first player who activates the challenge starts the run and is its first participant.
    public SocialChallengeRun(Guid challengeId, Guid firstPlayerId, DateTime startedAt, DateTime endsAt)
        : base(Guid.NewGuid())
    {
        if (endsAt <= startedAt)
        {
            throw new DomainException("A run must end after it starts.");
        }

        ChallengeId = challengeId;
        StartedAt = startedAt;
        EndsAt = endsAt;
        Status = SocialChallengeRunStatus.Active;
        _participants.Add(new SocialChallengeParticipant(firstPlayerId, startedAt));
    }

    public void Join(Guid playerId, DateTime now)
    {
        EnsureIsActiveAt(now);
        if (HasParticipant(playerId))
        {
            throw new DomainException("You are already in this run.");
        }

        _participants.Add(new SocialChallengeParticipant(playerId, now));
    }

    public void RecordCheck(Guid playerId, DateTime now)
    {
        EnsureIsActiveAt(now);

        GetParticipant(playerId).RecordCheck(now);
    }

    public void Remove(Guid playerId)
    {
        _participants.Remove(GetParticipant(playerId));
    }

    public void RemoveInactive(DateTime now)
    {
        _participants.RemoveAll(participant => now - participant.LastCheckedAt > MaximumTimeWithoutCheck);
    }

    // Does nothing while there are not enough participants; otherwise the run is solved for everyone in it at this moment.
    public void Solve(int xp, int requiredParticipants, DateTime now)
    {
        EnsureIsActiveAt(now);
        if (_participants.Count < requiredParticipants)
        {
            return;
        }

        Status = SocialChallengeRunStatus.Solved;
        SolvedAt = now;
        EarnedXp = xp;
    }

    public void Expire(DateTime now)
    {
        if (Status != SocialChallengeRunStatus.Active || now < EndsAt)
        {
            throw new DomainException("Only an active run whose time window has ended can expire.");
        }

        Status = SocialChallengeRunStatus.Expired;
    }

    // A run is expired as soon as its end passes, even before a check or an activation writes the Expired status.
    public bool IsExpiredAt(DateTime now)
    {
        return Status == SocialChallengeRunStatus.Expired
            || (Status == SocialChallengeRunStatus.Active && now >= EndsAt);
    }

    public bool HasParticipant(Guid playerId)
    {
        return _participants.Any(participant => participant.PlayerId == playerId);
    }

    private SocialChallengeParticipant GetParticipant(Guid playerId)
    {
        var participant = _participants.FirstOrDefault(candidate => candidate.PlayerId == playerId);
        if (participant is null)
        {
            throw new DomainException("You are not in this run.");
        }
        return participant;
    }

    private void EnsureIsActiveAt(DateTime now)
    {
        if (Status != SocialChallengeRunStatus.Active || now >= EndsAt)
        {
            throw new DomainException("This run is closed.");
        }
    }
}
