using Games.Application.Locations;

namespace Games.Application.LocationChallengeSolving;

// Only the Active state fills the fields after State; Unavailable and NotActivated leave them null.
public sealed record LocationChallengeProgressDto(
    Guid ChallengeId,
    LocationChallengeProgressState State,
    string? Name,
    LocationDto? Center,
    int? SearchRadiusInMeters,
    List<HiddenPointProgressDto>? HiddenPoints);
