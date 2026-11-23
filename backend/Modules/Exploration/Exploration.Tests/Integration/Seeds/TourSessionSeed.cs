using Exploration.Domain.Locations;
using Exploration.Domain.TourSessions;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSessionSeed
{
    public static readonly TourSession ExplorersActiveFortressUnderground;
    public static readonly TourSession SecondExplorersActiveKnezMihailovaWithFirstKeyPointReached;
    public static readonly TourSession BuyersCompletedKalemegdan;

    static TourSessionSeed()
    {
        ExplorersActiveFortressUnderground = new(WellKnownUsers.Explorer,
            TourSeed.PublishedModeratelyPricedFortress.Id, new Location(45.2671, 19.8335));

        SecondExplorersActiveKnezMihailovaWithFirstKeyPointReached = new(WellKnownUsers.SecondExplorer,
            TourSeed.ArchivedKnezMihailovaWithSecondKeyPointAtSecondExplorersPosition.Id, new Location(44.8164, 20.4600));
        SecondExplorersActiveKnezMihailovaWithFirstKeyPointReached.CompleteKeyPoint(
            TourSeed.ArchivedKnezMihailovaWithSecondKeyPointAtSecondExplorersPosition.KeyPoints[0].Id, false);

        BuyersCompletedKalemegdan = new(TourPurchaseSeed.Buyer,
            TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id, new Location(44.8231, 20.4504));
        BuyersCompletedKalemegdan.CompleteKeyPoint(TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.KeyPoints[0].Id, false);
        BuyersCompletedKalemegdan.CompleteKeyPoint(TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.KeyPoints[1].Id, true);
    }

    public static TourSession[] All => [ExplorersActiveFortressUnderground, SecondExplorersActiveKnezMihailovaWithFirstKeyPointReached, BuyersCompletedKalemegdan];
}
