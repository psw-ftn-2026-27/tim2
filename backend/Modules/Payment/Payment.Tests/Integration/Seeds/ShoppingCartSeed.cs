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
        ExplorersCartWithGalleryAndFortress.AddItem(ProductSeed.CheapGallery);
        ExplorersCartWithGalleryAndFortress.AddItem(ProductSeed.ModeratelyPricedFortress);

        SecondExplorersCartWithVineyards = new(WellKnownUsers.SecondExplorer);
        SecondExplorersCartWithVineyards.AddItem(ProductSeed.Vineyards);

        WealthyBuyersCartWithGalleryAndFortress = new(WalletSeed.WealthyBuyer);
        WealthyBuyersCartWithGalleryAndFortress.AddItem(ProductSeed.CheapGallery);
        WealthyBuyersCartWithGalleryAndFortress.AddItem(ProductSeed.ModeratelyPricedFortress);

        BuyerOfArchivedToursCartWithGalleryAndArchivedCastle = new(WalletSeed.BuyerOfArchivedTour);
        BuyerOfArchivedToursCartWithGalleryAndArchivedCastle.AddItem(ProductSeed.CheapGallery);
        BuyerOfArchivedToursCartWithGalleryAndArchivedCastle.AddItem(ProductSeed.ArchivedCastle);
    }

    public static ShoppingCart[] All => [ExplorersCartWithGalleryAndFortress, SecondExplorersCartWithVineyards, WealthyBuyersCartWithGalleryAndFortress, BuyerOfArchivedToursCartWithGalleryAndArchivedCastle];
}
