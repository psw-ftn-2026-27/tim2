using Games.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record QuizLocation
{
    private const int MinimumRangeInMeters = 10;
    private const int MaximumRangeInMeters = 1000;

    public Location Location { get; private init; }
    public int RangeInMeters { get; private init; }

    private QuizLocation()
    {
        Location = null!;
    }

    public QuizLocation(Location location, int rangeInMeters)
    {
        if (location is null)
        {
            throw new DomainException("A quiz location requires coordinates.");
        }
        if (rangeInMeters < MinimumRangeInMeters || rangeInMeters > MaximumRangeInMeters)
        {
            throw new DomainException($"A quiz range must be from {MinimumRangeInMeters} to {MaximumRangeInMeters} meters.");
        }

        Location = location;
        RangeInMeters = rangeInMeters;
    }
}
