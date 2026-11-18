using Games.Domain.LocationChallengeAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallengeAttempt ThirdExplorersStrandBeachInCurrentRound;
    public static readonly LocationChallengeAttempt ThirdExplorersSynagogueCourtyardFromFirstRound;
    public static readonly LocationChallengeAttempt ExplorersFishermansIslandBeforeItWasRetired;
    public static readonly LocationChallengeAttempt ExplorersLimanParkWithoutTheFirstPoint;
    public static readonly LocationChallengeAttempt PlayerWithoutPositionsLimanParkInCurrentRound;
    public static readonly LocationChallengeAttempt SecondExplorersSynagogueSquareSolvedInCurrentRound;
    public static readonly LocationChallengeAttempt SecondExplorersSynagogueCourtyardSolvedInFirstRound;
    public static readonly LocationChallengeAttempt ExplorersDanubeParkUnfinishedInFirstRound;
    public static readonly LocationChallengeAttempt ThirdExplorersCathedralSolvedInFirstRound;

    static LocationChallengeAttemptSeed()
    {
        ThirdExplorersStrandBeachInCurrentRound = new(
            LocationChallengeSeed.PublishedStrandBeach.Id,
            WellKnownUsers.ThirdExplorer,
            LocationChallengeSeed.PublishedStrandBeach.CurrentRound,
            LocationChallengeSeed.PublishedStrandBeach.SearchRadiusInMeters(40),
            new DateTime(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc));

        // Activated in the first round; the challenge was repeated and published again, so this attempt no longer counts.
        ThirdExplorersSynagogueCourtyardFromFirstRound = new(
            LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.Id,
            WellKnownUsers.ThirdExplorer,
            1,
            LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.SearchRadiusInMeters(70),
            new DateTime(2026, 11, 15, 11, 0, 0, DateTimeKind.Utc));

        ExplorersFishermansIslandBeforeItWasRetired = new(
            LocationChallengeSeed.RetiredFishermansIsland.Id,
            WellKnownUsers.Explorer,
            LocationChallengeSeed.RetiredFishermansIsland.CurrentRound,
            LocationChallengeSeed.RetiredFishermansIsland.SearchRadiusInMeters(10),
            new DateTime(2026, 11, 16, 15, 0, 0, DateTimeKind.Utc));

        // Every point is discovered except the first one, which is at the seeded position of Explorer.
        ExplorersLimanParkWithoutTheFirstPoint = new(
            LocationChallengeSeed.PublishedLimanPark.Id,
            WellKnownUsers.Explorer,
            LocationChallengeSeed.PublishedLimanPark.CurrentRound,
            LocationChallengeSeed.PublishedLimanPark.SearchRadiusInMeters(50),
            new DateTime(2026, 11, 17, 16, 0, 0, DateTimeKind.Utc));
        ExplorersLimanParkWithoutTheFirstPoint.RecordDiscovery(
            [2, 3],
            LocationChallengeSeed.PublishedLimanPark.HiddenPoints.Count,
            LocationChallengeSeed.PublishedLimanPark.Xp,
            new DateTime(2026, 11, 17, 16, 20, 0, DateTimeKind.Utc));

        // The player is not one of WellKnownUsers and has no seeded position, so a check by this player has no position to use.
        PlayerWithoutPositionsLimanParkInCurrentRound = new(
            LocationChallengeSeed.PublishedLimanPark.Id,
            Guid.Parse("00000000-0000-0000-0000-0000000000a1"),
            LocationChallengeSeed.PublishedLimanPark.CurrentRound,
            LocationChallengeSeed.PublishedLimanPark.SearchRadiusInMeters(20),
            new DateTime(2026, 11, 17, 17, 0, 0, DateTimeKind.Utc));

        SecondExplorersSynagogueSquareSolvedInCurrentRound = new(
            LocationChallengeSeed.PublishedSynagogueSquare.Id,
            WellKnownUsers.SecondExplorer,
            LocationChallengeSeed.PublishedSynagogueSquare.CurrentRound,
            LocationChallengeSeed.PublishedSynagogueSquare.SearchRadiusInMeters(60),
            new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc));
        SecondExplorersSynagogueSquareSolvedInCurrentRound.RecordDiscovery(
            [1],
            LocationChallengeSeed.PublishedSynagogueSquare.HiddenPoints.Count,
            LocationChallengeSeed.PublishedSynagogueSquare.Xp,
            new DateTime(2026, 11, 16, 8, 10, 0, DateTimeKind.Utc));
        SecondExplorersSynagogueSquareSolvedInCurrentRound.RecordDiscovery(
            [2],
            LocationChallengeSeed.PublishedSynagogueSquare.HiddenPoints.Count,
            LocationChallengeSeed.PublishedSynagogueSquare.Xp,
            new DateTime(2026, 11, 16, 8, 25, 0, DateTimeKind.Utc));

        // Solved in the first round; the challenge was repeated and published again, so this attempt no longer counts.
        SecondExplorersSynagogueCourtyardSolvedInFirstRound = new(
            LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.Id,
            WellKnownUsers.SecondExplorer,
            1,
            LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.SearchRadiusInMeters(30),
            new DateTime(2026, 11, 14, 9, 0, 0, DateTimeKind.Utc));
        SecondExplorersSynagogueCourtyardSolvedInFirstRound.RecordDiscovery(
            [1, 2],
            LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.HiddenPoints.Count,
            LocationChallengeSeed.RepublishedSynagogueCourtyardInSecondRound.Xp,
            new DateTime(2026, 11, 14, 9, 30, 0, DateTimeKind.Utc));

        ExplorersDanubeParkUnfinishedInFirstRound = new(
            LocationChallengeSeed.ArchivedDanubeParkWithAnUnfinishedAttempt.Id,
            WellKnownUsers.Explorer,
            1,
            LocationChallengeSeed.ArchivedDanubeParkWithAnUnfinishedAttempt.SearchRadiusInMeters(40),
            new DateTime(2026, 11, 13, 10, 0, 0, DateTimeKind.Utc));

        // Solved in the first round; the challenge was repeated, published again, and archived in its second round.
        ThirdExplorersCathedralSolvedInFirstRound = new(
            LocationChallengeSeed.ArchivedCathedralInSecondRoundSolvedInFirstRound.Id,
            WellKnownUsers.ThirdExplorer,
            1,
            LocationChallengeSeed.ArchivedCathedralInSecondRoundSolvedInFirstRound.SearchRadiusInMeters(25),
            new DateTime(2026, 11, 12, 14, 0, 0, DateTimeKind.Utc));
        ThirdExplorersCathedralSolvedInFirstRound.RecordDiscovery(
            [1, 2],
            LocationChallengeSeed.ArchivedCathedralInSecondRoundSolvedInFirstRound.HiddenPoints.Count,
            LocationChallengeSeed.ArchivedCathedralInSecondRoundSolvedInFirstRound.Xp,
            new DateTime(2026, 11, 12, 14, 40, 0, DateTimeKind.Utc));
    }

    public static LocationChallengeAttempt[] All => [ThirdExplorersStrandBeachInCurrentRound, ThirdExplorersSynagogueCourtyardFromFirstRound, ExplorersFishermansIslandBeforeItWasRetired, ExplorersLimanParkWithoutTheFirstPoint, PlayerWithoutPositionsLimanParkInCurrentRound, SecondExplorersSynagogueSquareSolvedInCurrentRound, SecondExplorersSynagogueCourtyardSolvedInFirstRound, ExplorersDanubeParkUnfinishedInFirstRound, ThirdExplorersCathedralSolvedInFirstRound];
}
