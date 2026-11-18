using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record TimeWindow
{
    private static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumDurationExclusive = TimeSpan.FromHours(24);

    // The start is entered as local time at the challenge's place. The zone is one constant for all challenges.
    // The IANA name, not a fixed offset, so the switch between winter and summer time is respected.
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

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

    // The window repeats every day; this asks whether the given moment falls in today's (or, past midnight, yesterday's) window.
    public bool Contains(DateTime utcNow)
    {
        var start = StartBefore(utcNow);
        return utcNow >= start && utcNow < start.Add(Duration);
    }

    // The end of the window that the given moment falls in, in UTC.
    public DateTime EndAfter(DateTime utcNow)
    {
        return StartBefore(utcNow).Add(Duration);
    }

    // The start of the window occurrence the moment belongs to, in UTC. For a window that crosses midnight,
    // a local time before the start belongs to the window that started yesterday.
    private DateTime StartBefore(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("A time window compares only UTC moments.");
        }

        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, Zone);
        var startDate = DateOnly.FromDateTime(localNow);
        var crossesMidnight = Start.ToTimeSpan() + Duration > TimeSpan.FromDays(1);
        if (crossesMidnight && TimeOnly.FromDateTime(localNow) < Start)
        {
            startDate = startDate.AddDays(-1);
        }

        var localStart = startDate.ToDateTime(Start, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localStart, Zone);
    }
}
