using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallengeManagement;

public sealed record LocationChallengeDto(
    Guid Id,
    string Name,
    int Xp,
    LocationChallengeStatus Status,
    LocationDto StartLocation,
    List<HiddenPointDto> HiddenPoints);
