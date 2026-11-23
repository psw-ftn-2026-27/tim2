namespace Games.Application.NearbyChallenges;

// The part of a nearby challenge that only a social challenge has. WindowStart is local time at the challenge's place.
public sealed record NearbySocialChallengeDto(
    int RequiredParticipants,
    TimeOnly WindowStart,
    bool IsWindowOpen);
