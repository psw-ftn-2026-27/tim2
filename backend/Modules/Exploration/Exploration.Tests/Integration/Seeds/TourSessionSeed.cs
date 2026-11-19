using Exploration.Domain.Locations;
using Exploration.Domain.TourSessions;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSessionSeed
{
    public static readonly TourSession ExplorersActiveFortressUnderground = new(WellKnownUsers.Explorer,
        TourSeed.PublishedModeratelyPricedFortress.Id, new Location(45.2671, 19.8335));

    public static TourSession[] All => [ExplorersActiveFortressUnderground];
}
