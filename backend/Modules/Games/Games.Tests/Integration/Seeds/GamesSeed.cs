namespace Games.Tests.Integration.Seeds;

internal static class GamesSeed
{
    public static object[] All => [.. SocialChallengeSeed.All, .. QuizSeed.All, .. LocationChallengeSeed.All, .. QuizAttemptSeed.All, .. LocationChallengeAttemptSeed.All];
}
