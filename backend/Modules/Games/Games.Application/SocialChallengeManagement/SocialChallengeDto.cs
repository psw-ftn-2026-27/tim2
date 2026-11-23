using Games.Application.Locations;
using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengeManagement;

// End is computed from Start and Duration; Location is null while the challenge has none.
public sealed record SocialChallengeDto(
    Guid Id,
    string Name,
    TimeOnly Start,
    TimeSpan Duration,
    TimeOnly End,
    int RequiredParticipants,
    int RangeInMeters,
    int Xp,
    string CompletionMessage,
    SocialChallengeStatus Status,
    LocationDto? Location);
