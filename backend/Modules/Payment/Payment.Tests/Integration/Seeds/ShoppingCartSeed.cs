using Payment.Domain.ShoppingCarts;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly ShoppingCart ExplorersCartWithGalleryAndFortress;
    public static readonly ShoppingCart SecondExplorersCartWithVineyards;

    static ShoppingCartSeed()
    {
        ExplorersCartWithGalleryAndFortress = new(WellKnownUsers.Explorer);
        ExplorersCartWithGalleryAndFortress.AddItem(ProductSeed.CheapGallery);
        ExplorersCartWithGalleryAndFortress.AddItem(ProductSeed.ModeratelyPricedFortress);

        SecondExplorersCartWithVineyards = new(WellKnownUsers.SecondExplorer);
        SecondExplorersCartWithVineyards.AddItem(ProductSeed.Vineyards);
    }

    public static ShoppingCart[] All => [ExplorersCartWithGalleryAndFortress, SecondExplorersCartWithVineyards];
}
