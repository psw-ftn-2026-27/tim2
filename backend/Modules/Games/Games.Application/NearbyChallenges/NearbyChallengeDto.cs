using Games.Application.Challenges;

namespace Games.Application.NearbyChallenges;

// MetersToRange is 0 when the player is inside the challenge range.
// The fields after IsSolved belong to a social challenge and are null for a quiz. WindowStart is local time at the challenge's place;
// EarnedXp is filled only for a solved social challenge.
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
    int? RequiredParticipants,
    TimeOnly? WindowStart,
    bool? IsWindowOpen,
    bool? IsInActiveRun,
    int? EarnedXp);
