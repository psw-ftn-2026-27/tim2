using Games.Application.Locations;

namespace Games.Application.LocationChallengeSolving;

// Number counts from 1. Location stays null until the player discovers the point, so an undiscovered location is never sent.
public sealed record HiddenPointProgressDto(int Number, string ImageUrl, LocationDto? Location);
