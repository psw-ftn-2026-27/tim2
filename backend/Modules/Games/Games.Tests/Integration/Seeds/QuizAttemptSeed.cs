using Games.Domain.QuizAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizAttemptSeed
{
    public static readonly QuizAttempt ExplorersDanubeParkAfterFirstAnswer;
    public static readonly QuizAttempt ExplorersFishermansIslandSolvedWithOneOfTwo;
    public static readonly QuizAttempt ThirdExplorersDanubeParkSolvedWithTwoOfThree;
    public static readonly QuizAttempt SecondExplorersSynagogueWithoutAnswers;

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

        ThirdExplorersDanubeParkSolvedWithTwoOfThree = new(
            QuizSeed.PublishedDanubePark.Id,
            WellKnownUsers.ThirdExplorer,
            QuizSeed.PublishedDanubePark.Questions.Count,
            new DateTime(2026, 11, 14, 10, 0, 0, DateTimeKind.Utc));
        ThirdExplorersDanubeParkSolvedWithTwoOfThree.RecordAnswer(
            new QuizAnswer(1, true, new DateTime(2026, 11, 14, 10, 1, 0, DateTimeKind.Utc)),
            QuizSeed.PublishedDanubePark.Xp);
        ThirdExplorersDanubeParkSolvedWithTwoOfThree.RecordAnswer(
            new QuizAnswer(0, true, new DateTime(2026, 11, 14, 10, 2, 0, DateTimeKind.Utc)),
            QuizSeed.PublishedDanubePark.Xp);
        ThirdExplorersDanubeParkSolvedWithTwoOfThree.RecordAnswer(
            new QuizAnswer(1, false, new DateTime(2026, 11, 14, 10, 3, 0, DateTimeKind.Utc)),
            QuizSeed.PublishedDanubePark.Xp);

        // PublishedSynagogue has a single question, so the next answer is the last one.
        SecondExplorersSynagogueWithoutAnswers = new(
            QuizSeed.PublishedSynagogue.Id,
            WellKnownUsers.SecondExplorer,
            QuizSeed.PublishedSynagogue.Questions.Count,
            new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc));
    }

    public static QuizAttempt[] All => [ExplorersDanubeParkAfterFirstAnswer, ExplorersFishermansIslandSolvedWithOneOfTwo, ThirdExplorersDanubeParkSolvedWithTwoOfThree, SecondExplorersSynagogueWithoutAnswers];
}
