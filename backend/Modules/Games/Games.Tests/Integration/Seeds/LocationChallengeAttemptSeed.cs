using Games.Domain.LocationChallengeAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallengeAttempt ThirdExplorersStrandBeachInCurrentRound;
    public static readonly LocationChallengeAttempt ThirdExplorersSynagogueCourtyardFromFirstRound;
    public static readonly LocationChallengeAttempt ExplorersFishermansIslandBeforeItWasRetired;

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
    }

    public static LocationChallengeAttempt[] All => [ThirdExplorersStrandBeachInCurrentRound, ThirdExplorersSynagogueCourtyardFromFirstRound, ExplorersFishermansIslandBeforeItWasRetired];
}
