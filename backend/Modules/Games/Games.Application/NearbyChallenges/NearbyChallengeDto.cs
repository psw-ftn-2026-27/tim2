using Games.Application.Challenges;

namespace Games.Application.NearbyChallenges;

// MetersToRange is 0 when the player is inside the challenge range. EarnedXp stays null until the player solves the challenge.
public sealed record NearbyChallengeDto(
    Guid Id,
    ChallengeType Type,
    string Name,
    int Xp,
    double Latitude,
    double Longitude,
    double DistanceInMeters,
    int MetersToRange,
    bool IsStarted,
    bool IsSolved,
    int? EarnedXp);
