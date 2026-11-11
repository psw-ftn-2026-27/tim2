using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    private const int MinimumNameLength = 3;
    private const int MaximumNameLength = 60;
    private const int MinimumRequiredParticipants = 2;
    private const int MaximumRequiredParticipants = 100;
    private const int MinimumRangeInMeters = 10;
    private const int MaximumRangeInMeters = 1000;
    private const int MinimumXp = 1;
    private const int MaximumXp = 1000;
    private const int MinimumCompletionMessageLength = 1;
    private const int MaximumCompletionMessageLength = 200;

    public string Name { get; private set; }
    public TimeWindow Window { get; private set; }
    public int RequiredParticipants { get; private set; }
    public int RangeInMeters { get; private set; }
    public int Xp { get; private set; }
    public string CompletionMessage { get; private set; }
    public SocialChallengeStatus Status { get; private set; }

    private SocialChallenge()
    {
        Name = null!;
        Window = null!;
        CompletionMessage = null!;
    }

    public SocialChallenge(
        string name,
        TimeOnly start,
        TimeSpan duration,
        int requiredParticipants,
        int rangeInMeters,
        int xp,
        string completionMessage)
        : base(Guid.NewGuid())
    {
        var window = new TimeWindow(start, duration);
        Validate(name, requiredParticipants, rangeInMeters, xp, completionMessage);

        Name = name.Trim();
        Window = window;
        RequiredParticipants = requiredParticipants;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        CompletionMessage = completionMessage.Trim();
        Status = SocialChallengeStatus.Draft;
    }

    public void Update(
        string name,
        TimeOnly start,
        TimeSpan duration,
        int requiredParticipants,
        int rangeInMeters,
        int xp,
        string completionMessage)
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can be changed.");
        }

        var window = new TimeWindow(start, duration);
        Validate(name, requiredParticipants, rangeInMeters, xp, completionMessage);

        Name = name.Trim();
        Window = window;
        RequiredParticipants = requiredParticipants;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        CompletionMessage = completionMessage.Trim();
    }

    public void EnsureCanBeDeleted()
    {
        if (Status != SocialChallengeStatus.Draft && Status != SocialChallengeStatus.Archived)
        {
            throw new DomainException("Only a draft or archived social challenge can be deleted.");
        }
    }

    private static void Validate(string name, int requiredParticipants, int rangeInMeters, int xp, string completionMessage)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length < MinimumNameLength || trimmedName.Length > MaximumNameLength)
        {
            throw new DomainException($"A social challenge name must have from {MinimumNameLength} to {MaximumNameLength} characters.");
        }
        if (requiredParticipants < MinimumRequiredParticipants || requiredParticipants > MaximumRequiredParticipants)
        {
            throw new DomainException($"A social challenge requires from {MinimumRequiredParticipants} to {MaximumRequiredParticipants} participants.");
        }
        if (rangeInMeters < MinimumRangeInMeters || rangeInMeters > MaximumRangeInMeters)
        {
            throw new DomainException($"A social challenge range must be from {MinimumRangeInMeters} to {MaximumRangeInMeters} meters.");
        }
        if (xp < MinimumXp || xp > MaximumXp)
        {
            throw new DomainException($"A social challenge must carry from {MinimumXp} to {MaximumXp} XP.");
        }
        var trimmedCompletionMessage = completionMessage?.Trim() ?? string.Empty;
        if (trimmedCompletionMessage.Length < MinimumCompletionMessageLength || trimmedCompletionMessage.Length > MaximumCompletionMessageLength)
        {
            throw new DomainException($"A social challenge completion message must have from {MinimumCompletionMessageLength} to {MaximumCompletionMessageLength} characters.");
        }
    }
}
