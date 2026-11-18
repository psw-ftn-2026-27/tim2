using Games.Application.Locations;

namespace Games.Application.SocialChallengeSolving;

// ParticipantCount and LastCheckedAt describe the run the player is in, as the last check left it.
// The result fields (CompletionMessage, EarnedXp, SolvedAt) are filled only in the Solved state.
public sealed record SocialChallengeProgressDto(
    Guid ChallengeId,
    string Name,
    LocationDto? Location,
    int RangeInMeters,
    int RequiredParticipants,
    int ParticipantCount,
    DateTime? LastCheckedAt,
    SocialChallengeProgressState State,
    string? CompletionMessage,
    int? EarnedXp,
    DateTime? SolvedAt);
