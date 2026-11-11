using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengeManagement;

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
    SocialChallengeStatus Status);
