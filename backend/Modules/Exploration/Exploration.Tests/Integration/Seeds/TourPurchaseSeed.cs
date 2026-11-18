using Payment.Domain.TourPurchaseTokens;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

/// <summary>
/// Purchase tokens in the payment schema of the Exploration test database, so that the real
/// IPaymentApi recognises a purchase. Each test project has its own database, so this does not
/// touch Payment.Tests. Approved by the platform team; every Exploration feature that needs a
/// purchased tour adds its tokens here, because a second reseed of PaymentDbContext would
/// truncate these.
/// </summary>
internal static class TourPurchaseSeed
{
    public static readonly Guid Buyer = WellKnownUsers.ThirdExplorer;

    public static readonly TourPurchaseToken BuyersKalemegdan = new(Buyer, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Id, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Name, TourSeed.PublishedKalemegdanWithVisibleKeyPointInBelgrade.Price);
    public static readonly TourPurchaseToken BuyersSkadarlija = new(Buyer, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Id, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Name, TourSeed.PublishedSkadarlijaWithOnlyHiddenKeyPointInBelgrade.Price);
    public static readonly TourPurchaseToken BuyersArchivedTasmajdan = new(Buyer, TourSeed.ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Id, TourSeed.ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Name, TourSeed.ArchivedTasmajdanWithVisibleKeyPointInBelgrade.Price);
    public static readonly TourPurchaseToken BuyersSynagogue = new(Buyer, TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint.Id, TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint.Name, TourSeed.PublishedSynagogueWithSecretOnVisibleKeyPoint.Price);

    public static readonly TourPurchaseToken ExplorersFortressUnderground = new(WellKnownUsers.Explorer, TourSeed.PublishedModeratelyPricedFortress.Id, TourSeed.PublishedModeratelyPricedFortress.Name, TourSeed.PublishedModeratelyPricedFortress.Price);
    public static readonly TourPurchaseToken ExplorersGalleries = new(WellKnownUsers.Explorer, TourSeed.PublishedCheapGallery.Id, TourSeed.PublishedCheapGallery.Name, TourSeed.PublishedCheapGallery.Price);

    public static TourPurchaseToken[] All => [BuyersKalemegdan, BuyersSkadarlija, BuyersArchivedTasmajdan, BuyersSynagogue, ExplorersFortressUnderground, ExplorersGalleries];
}
