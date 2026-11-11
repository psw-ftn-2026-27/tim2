using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourPreferenceSeed
{
    public static readonly TourPreference ExplorersEasyWalkingHistory = new(WellKnownUsers.Explorer,
        new PreferenceCriteria([TourDifficulty.Easy], [TransportMode.Walking], ["istorija"]), true);
    public static readonly TourPreference SecondExplorersHardCarMountains = new(WellKnownUsers.SecondExplorer,
        new PreferenceCriteria([TourDifficulty.Hard], [TransportMode.Car], ["planina"]), false);

    public static TourPreference[] All => [ExplorersEasyWalkingHistory, SecondExplorersHardCarMountains];
}
