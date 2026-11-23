using Exploration.Domain.Locations;
using Exploration.Domain.TourLocationSearchRecords;
using Exploration.Domain.Tours;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourLocationSearchRecordSeed
{
    public static readonly TourLocationSearchRecord NoviSadTenKmEasyWalkingHistoryWithTwoResults = new(
        new SearchArea(new Location(45.2551, 19.8452), 10), "tvrđava", [TourDifficulty.Easy], ["istorija"],
        [TransportMode.Walking], 0, 2000, 1, 15, 2);

    public static TourLocationSearchRecord[] All => [NoviSadTenKmEasyWalkingHistoryWithTwoResults];
}
