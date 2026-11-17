using Games.Application.Challenges;

namespace Games.Application.ChallengeOverview;

// One row of the shared overview. The social challenge columns are null for a quiz and a location challenge,
// and Latitude and Longitude are also null for a social challenge without a location.
public sealed record ChallengeDto(
    Guid Id,
    ChallengeType Type,
    string Name,
    int Xp,
    string Status,
    TimeOnly? Start,
    TimeSpan? Duration,
    TimeOnly? End,
    int? RequiredParticipants,
    int? RangeInMeters,
    string? CompletionMessage,
    double? Latitude,
    double? Longitude);
