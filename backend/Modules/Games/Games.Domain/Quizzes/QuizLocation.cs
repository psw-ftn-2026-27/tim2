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
        if (latitude < MinimumLatitude || latitude > MaximumLatitude)
        {
            throw new DomainException($"A quiz location latitude must be from {MinimumLatitude} to {MaximumLatitude}.");
        }
        if (longitude < MinimumLongitude || longitude > MaximumLongitude)
        {
            throw new DomainException($"A quiz location longitude must be from {MinimumLongitude} to {MaximumLongitude}.");
        }
        if (rangeInMeters < MinimumRangeInMeters || rangeInMeters > MaximumRangeInMeters)
        {
            throw new DomainException($"A quiz range must be from {MinimumRangeInMeters} to {MaximumRangeInMeters} meters.");
        }

        Latitude = latitude;
        Longitude = longitude;
        RangeInMeters = rangeInMeters;
    }
}
