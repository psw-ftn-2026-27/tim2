using Games.Domain.QuizAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizAttemptSeed
{
    public static readonly QuizAttempt ExplorersDanubeParkAfterFirstAnswer;
    public static readonly QuizAttempt ExplorersFishermansIslandSolvedWithOneOfTwo;

    static QuizAttemptSeed()
    {
        ExplorersDanubeParkAfterFirstAnswer = new(
            QuizSeed.PublishedDanubePark.Id,
            WellKnownUsers.Explorer,
            QuizSeed.PublishedDanubePark.Questions.Count,
            new DateTime(2026, 11, 16, 9, 0, 0, DateTimeKind.Utc));
        ExplorersDanubeParkAfterFirstAnswer.RecordAnswer(
            new QuizAnswer(1, true, new DateTime(2026, 11, 16, 9, 1, 0, DateTimeKind.Utc)),
            QuizSeed.PublishedDanubePark.Xp);

        // Solved while the quiz carried 60 XP; the quiz was later repeated and changed to 90 XP.
        ExplorersFishermansIslandSolvedWithOneOfTwo = new(
            QuizSeed.RepeatedFishermansIslandWithMoreXp.Id,
            WellKnownUsers.Explorer,
            2,
            new DateTime(2026, 11, 15, 14, 0, 0, DateTimeKind.Utc));
        ExplorersFishermansIslandSolvedWithOneOfTwo.RecordAnswer(
            new QuizAnswer(0, true, new DateTime(2026, 11, 15, 14, 1, 0, DateTimeKind.Utc)),
            60);
        ExplorersFishermansIslandSolvedWithOneOfTwo.RecordAnswer(
            new QuizAnswer(1, false, new DateTime(2026, 11, 15, 14, 2, 0, DateTimeKind.Utc)),
            60);
    }

    public static QuizAttempt[] All => [ExplorersDanubeParkAfterFirstAnswer, ExplorersFishermansIslandSolvedWithOneOfTwo];
}
