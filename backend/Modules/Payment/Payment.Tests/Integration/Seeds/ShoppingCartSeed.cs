using Payment.Domain.ShoppingCarts;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly ShoppingCart ExplorersCartWithGalleryAndFortress;
    public static readonly ShoppingCart SecondExplorersCartWithVineyards;
    public static readonly ShoppingCart WealthyBuyersCartWithGalleryAndFortress;
    public static readonly ShoppingCart BuyerOfArchivedToursCartWithGalleryAndArchivedCastle;

    static ShoppingCartSeed()
    {
        ExplorersCartWithGalleryAndFortress = new(WellKnownUsers.Explorer);
        ExplorersCartWithGalleryAndFortress.AddItem(ProductSeed.CheapGallery, false);
        ExplorersCartWithGalleryAndFortress.AddItem(ProductSeed.ModeratelyPricedFortress, false);

        SecondExplorersCartWithVineyards = new(WellKnownUsers.SecondExplorer);
        SecondExplorersCartWithVineyards.AddItem(ProductSeed.Vineyards, false);

        WealthyBuyersCartWithGalleryAndFortress = new(WalletSeed.WealthyBuyer);
        WealthyBuyersCartWithGalleryAndFortress.AddItem(ProductSeed.CheapGallery, false);
        WealthyBuyersCartWithGalleryAndFortress.AddItem(ProductSeed.ModeratelyPricedFortress, false);

        BuyerOfArchivedToursCartWithGalleryAndArchivedCastle = new(WalletSeed.BuyerOfArchivedTour);
        BuyerOfArchivedToursCartWithGalleryAndArchivedCastle.AddItem(ProductSeed.CheapGallery, false);
        BuyerOfArchivedToursCartWithGalleryAndArchivedCastle.AddItem(ProductSeed.ArchivedCastle, false);
    }

    public static ShoppingCart[] All => [ExplorersCartWithGalleryAndFortress, SecondExplorersCartWithVineyards, WealthyBuyersCartWithGalleryAndFortress, BuyerOfArchivedToursCartWithGalleryAndArchivedCastle];
}
