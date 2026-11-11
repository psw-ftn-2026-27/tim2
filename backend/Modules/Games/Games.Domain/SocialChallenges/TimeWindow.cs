using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record TimeWindow
{
    private static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumDurationExclusive = TimeSpan.FromHours(24);

    public TimeOnly Start { get; private init; }
    public TimeSpan Duration { get; private init; }

    private TimeWindow() { }

    public TimeWindow(TimeOnly start, TimeSpan duration)
    {
        if (duration < MinimumDuration)
        {
            throw new DomainException($"A time window must last at least {MinimumDuration.TotalMinutes} minutes.");
        }
        if (duration >= MaximumDurationExclusive)
        {
            throw new DomainException($"A time window must last less than {MaximumDurationExclusive.TotalHours} hours.");
        }

        Start = start;
        Duration = duration;
    }

    public TimeOnly End()
    {
        return Start.Add(Duration);
    }
}
