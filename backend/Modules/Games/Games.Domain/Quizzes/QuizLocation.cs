using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record QuizLocation
{
    private const double MinimumLatitude = -90;
    private const double MaximumLatitude = 90;
    private const double MinimumLongitude = -180;
    private const double MaximumLongitude = 180;
    private const int MinimumRangeInMeters = 10;
    private const int MaximumRangeInMeters = 1000;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }
    public int RangeInMeters { get; private init; }

    private QuizLocation() { }

    public QuizLocation(double latitude, double longitude, int rangeInMeters)
    {
        EnsureWithin(latitude, MinimumLatitude, MaximumLatitude,
            $"A quiz location latitude must be from {MinimumLatitude} to {MaximumLatitude}.");
        EnsureWithin(longitude, MinimumLongitude, MaximumLongitude,
            $"A quiz location longitude must be from {MinimumLongitude} to {MaximumLongitude}.");
        EnsureWithin(rangeInMeters, MinimumRangeInMeters, MaximumRangeInMeters,
            $"A quiz range must be from {MinimumRangeInMeters} to {MaximumRangeInMeters} meters.");

        Latitude = latitude;
        Longitude = longitude;
        RangeInMeters = rangeInMeters;
    }

    // Reuses the distance formula of the location challenge's Location instead of repeating it here.
    public double DistanceInMetersTo(double latitude, double longitude)
    {
        return new Location(Latitude, Longitude).DistanceInMetersTo(new Location(latitude, longitude));
    }

    // Asks "is the value inside the bounds?" rather than "is it outside?", because every
    // comparison with double.NaN is false: an outside check would let NaN through.
    private static void EnsureWithin(double value, double minimum, double maximum, string message)
    {
        if (!(value >= minimum && value <= maximum))
        {
            throw new DomainException(message);
        }
    }
}
