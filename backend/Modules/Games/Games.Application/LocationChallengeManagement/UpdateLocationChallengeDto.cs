namespace Games.Application.LocationChallengeManagement;

public sealed record UpdateLocationChallengeDto(
    string Name,
    int Xp,
    LocationDto StartLocation,
    List<HiddenPointDto> HiddenPoints);
