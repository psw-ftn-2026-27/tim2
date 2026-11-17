using Exploration.Tests.Integration.Seeds;
using Payment.Domain.TourPurchaseTokens;

namespace Payment.Tests.Integration.Seeds;

internal static class TourPurchaseTokenSeed
{
    public static readonly TourPurchaseToken WealthyBuyersKalemegdan = new(WalletSeed.WealthyBuyer, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Name, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Price);
    public static readonly TourPurchaseToken WealthyBuyersSkadarlija = new(WalletSeed.WealthyBuyer, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Id, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Name, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Price);
    public static readonly TourPurchaseToken BuyerOfArchivedToursAvala = new(WalletSeed.BuyerOfArchivedTour, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Id, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Name, TourSeed.PublishedAvalaWithFirstVisibleKeyPointOutsideBelgrade.Price);

    public static TourPurchaseToken[] All => [WealthyBuyersKalemegdan, WealthyBuyersSkadarlija, BuyerOfArchivedToursAvala];
}
