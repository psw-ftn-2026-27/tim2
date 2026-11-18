using Games.Application.Locations;

namespace Games.Application.LocationChallengeSolving;

// Active and Solved fill the fields from Name to HiddenPoints; Unavailable and NotActivated leave them null.
// Only Solved fills SolvedAt and EarnedXp.
public sealed record LocationChallengeProgressDto(
    Guid ChallengeId,
    LocationChallengeProgressState State,
    string? Name,
    LocationDto? Center,
    int? SearchRadiusInMeters,
    List<HiddenPointProgressDto>? HiddenPoints,
    DateTime? SolvedAt,
    int? EarnedXp);
