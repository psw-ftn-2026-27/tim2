using Games.Application.Challenges;

namespace Games.Application.NearbyChallenges;

// MetersToRange is 0 when the player is inside the challenge range.
// IsStarted means an attempt exists for a quiz, and that the player is in the active run for a social challenge.
// EarnedXp is filled only once the player has solved the challenge.
// Social holds the fields only a social challenge has and is null for a quiz.
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
    int? EarnedXp,
    NearbySocialChallengeDto? Social);
