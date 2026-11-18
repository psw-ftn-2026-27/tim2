using Games.Domain.Locations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : AggregateRoot
{
    // Fixed by the system for every location challenge; kept in one place so it can become an administrator field later.
    public const int ActivationRangeInMeters = 10;

    private const int MinimumNameLength = 3;
    private const int MaximumNameLength = 60;
    private const int MinimumXp = 1;
    private const int MaximumXp = 1000;
    private const int MinimumHiddenPointCount = 1;
    private const int MaximumHiddenPointCount = 10;
    private const double MaximumDistanceFromStartInMeters = 1000;
    private const double MinimumDistanceFromStartInMeters = 20;
    private const double MinimumDistanceBetweenHiddenPointsInMeters = 20;

    private readonly List<HiddenPoint> _hiddenPoints = [];

    public string Name { get; private set; }
    public int Xp { get; private set; }
    public Location StartLocation { get; private set; }
    public LocationChallengeStatus Status { get; private set; }
    // Counts the publications; an attempt belongs to the current round when its round equals this one.
    public int CurrentRound { get; private set; }
    public IReadOnlyList<HiddenPoint> HiddenPoints => _hiddenPoints;

    private LocationChallenge()
    {
        Name = null!;
        StartLocation = null!;
    }

    public LocationChallenge(string name, int xp, Location startLocation, List<HiddenPoint> hiddenPoints)
        : base(Guid.NewGuid())
    {
        Validate(name, xp, startLocation, hiddenPoints);

        Name = name.Trim();
        Xp = xp;
        StartLocation = startLocation;
        _hiddenPoints.AddRange(hiddenPoints);
        Status = LocationChallengeStatus.Draft;
    }

    public void Update(string name, int xp, Location startLocation, List<HiddenPoint> hiddenPoints)
    {
        EnsureStatusIsOneOf(LocationChallengeStatus.Draft);

        Validate(name, xp, startLocation, hiddenPoints);

        Name = name.Trim();
        Xp = xp;
        StartLocation = startLocation;
        _hiddenPoints.Clear();
        _hiddenPoints.AddRange(hiddenPoints);
    }

    public void EnsureCanBeDeleted()
    {
        EnsureStatusIsOneOf(LocationChallengeStatus.Draft, LocationChallengeStatus.Archived);
    }

    public void Publish()
    {
        EnsureStatusIsOneOf(LocationChallengeStatus.Draft);

        Status = LocationChallengeStatus.Published;
        CurrentRound++;
    }

    public void Retire()
    {
        EnsureStatusIsOneOf(LocationChallengeStatus.Published);

        Status = LocationChallengeStatus.Retired;
    }

    public void Archive()
    {
        EnsureStatusIsOneOf(LocationChallengeStatus.Retired);

        Status = LocationChallengeStatus.Archived;
    }

    public void Repeat()
    {
        EnsureStatusIsOneOf(LocationChallengeStatus.Retired, LocationChallengeStatus.Archived);

        Status = LocationChallengeStatus.Draft;
    }

    public void EnsureCanBePlayed()
    {
        if (Status != LocationChallengeStatus.Published)
        {
            throw new DomainException("This location challenge is not available, so it cannot be played.");
        }
    }

    public bool IsWithinActivationRange(Location position)
    {
        return StartLocation.DistanceInMetersTo(position) <= ActivationRangeInMeters;
    }

    // The search area is centered on the start location, so it reaches the farthest hidden point plus the extra radius.
    // Rounded up, so a point on the edge stays inside the area.
    public int SearchRadiusInMeters(int extraRadiusInMeters)
    {
        var farthestDistance = _hiddenPoints.Max(point => point.Location.DistanceInMetersTo(StartLocation));
        return (int)Math.Ceiling(farthestDistance) + extraRadiusInMeters;
    }

    private void EnsureStatusIsOneOf(params LocationChallengeStatus[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(Status))
        {
            throw new DomainException($"This action is not allowed for a location challenge in the {Status} state.");
        }
    }

    private static void Validate(string name, int xp, Location startLocation, List<HiddenPoint> hiddenPoints)
    {
        ValidateName(name);
        ValidateXp(xp);
        ValidateStartLocation(startLocation);
        ValidateHiddenPointCount(hiddenPoints);
        ValidateDistancesFromStart(startLocation, hiddenPoints);
        ValidateDistancesBetweenHiddenPoints(hiddenPoints);
    }

    private static void ValidateName(string name)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length < MinimumNameLength || trimmedName.Length > MaximumNameLength)
        {
            throw new DomainException($"A location challenge name must have from {MinimumNameLength} to {MaximumNameLength} characters.");
        }
    }

    private static void ValidateXp(int xp)
    {
        if (xp < MinimumXp || xp > MaximumXp)
        {
            throw new DomainException($"A location challenge must carry from {MinimumXp} to {MaximumXp} XP.");
        }
    }

    private static void ValidateStartLocation(Location startLocation)
    {
        if (startLocation is null)
        {
            throw new DomainException("A location challenge requires a start location.");
        }
    }

    private static void ValidateHiddenPointCount(List<HiddenPoint> hiddenPoints)
    {
        var hiddenPointCount = hiddenPoints?.Count ?? 0;
        if (hiddenPointCount < MinimumHiddenPointCount || hiddenPointCount > MaximumHiddenPointCount)
        {
            throw new DomainException($"A location challenge must have from {MinimumHiddenPointCount} to {MaximumHiddenPointCount} hidden points.");
        }
    }

    // A point is named by its ordinal number, counted from 1, so the administrator knows which one to fix.
    private static void ValidateDistancesFromStart(Location startLocation, List<HiddenPoint> hiddenPoints)
    {
        for (var index = 0; index < hiddenPoints.Count; index++)
        {
            var distance = hiddenPoints[index].Location.DistanceInMetersTo(startLocation);
            if (distance > MaximumDistanceFromStartInMeters)
            {
                throw new DomainException($"Hidden point {index + 1} is {distance:F0} m from the start location, but it can be at most {MaximumDistanceFromStartInMeters} m away.");
            }
            if (distance < MinimumDistanceFromStartInMeters)
            {
                throw new DomainException($"Hidden point {index + 1} is {distance:F0} m from the start location, but it must be at least {MinimumDistanceFromStartInMeters} m away.");
            }
        }
    }

    private static void ValidateDistancesBetweenHiddenPoints(List<HiddenPoint> hiddenPoints)
    {
        for (var first = 0; first < hiddenPoints.Count; first++)
        {
            for (var second = first + 1; second < hiddenPoints.Count; second++)
            {
                var distance = hiddenPoints[first].Location.DistanceInMetersTo(hiddenPoints[second].Location);
                if (distance < MinimumDistanceBetweenHiddenPointsInMeters)
                {
                    throw new DomainException($"Hidden points {first + 1} and {second + 1} are {distance:F0} m apart, but they must be at least {MinimumDistanceBetweenHiddenPointsInMeters} m apart.");
                }
            }
        }
    }
}
