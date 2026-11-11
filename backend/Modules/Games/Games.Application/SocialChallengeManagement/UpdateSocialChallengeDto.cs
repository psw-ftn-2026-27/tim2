namespace Games.Application.SocialChallengeManagement;

public sealed record UpdateSocialChallengeDto(
    string Name,
    TimeOnly Start,
    TimeSpan Duration,
    int RequiredParticipants,
    int RangeInMeters,
    int Xp,
    string CompletionMessage);
