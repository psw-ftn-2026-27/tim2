using Exploration.Tests.Integration.Seeds;
using Payment.Domain.Products;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product CheapGallery = new(TourSeed.PublishedCheapGallery.Id, TourSeed.PublishedCheapGallery.Name, TourSeed.PublishedCheapGallery.AuthorId, TourSeed.PublishedCheapGallery.Price);
    public static readonly Product ModeratelyPricedFortress = new(TourSeed.PublishedModeratelyPricedFortress.Id, TourSeed.PublishedModeratelyPricedFortress.Name, TourSeed.PublishedModeratelyPricedFortress.AuthorId, TourSeed.PublishedModeratelyPricedFortress.Price);
    public static readonly Product Vineyards = new(TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, TourSeed.PublishedVineyards.AuthorId, TourSeed.PublishedVineyards.Price);
    public static readonly Product FreeParkWalkInNoCart = new(TourSeed.PublishedFreeParkWalk.Id, TourSeed.PublishedFreeParkWalk.Name, TourSeed.PublishedFreeParkWalk.AuthorId, TourSeed.PublishedFreeParkWalk.Price);
    public static readonly Product ArchivedCastle = new(TourSeed.ArchivedCastle.Id, TourSeed.ArchivedCastle.Name, TourSeed.ArchivedCastle.AuthorId, TourSeed.ArchivedCastle.Price);

    public static Product[] All => [CheapGallery, ModeratelyPricedFortress, Vineyards, FreeParkWalkInNoCart, ArchivedCastle];
}
