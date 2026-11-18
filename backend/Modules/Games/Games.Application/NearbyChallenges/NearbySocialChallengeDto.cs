namespace Games.Application.NearbyChallenges;

// The part of a nearby challenge that only a social challenge has. WindowStart is local time at the challenge's place;
// EarnedXp is filled only once the player has solved the challenge.
public sealed record NearbySocialChallengeDto(
    int RequiredParticipants,
    TimeOnly WindowStart,
    bool IsWindowOpen,
    int? EarnedXp);
