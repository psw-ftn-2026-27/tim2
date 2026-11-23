using Exploration.Tests.Integration.Seeds;
using Payment.Domain.Coupons;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CouponSeed
{
    public static readonly Coupon ExplorerVineyardsCoupon;
    public static readonly Coupon ExplorerAllToursCoupon;
    public static readonly Coupon SecondExplorerAllToursCoupon;

    static CouponSeed()
    {
        var now = DateTime.UtcNow;
        var expiresOn = DateOnly.FromDateTime(now).AddDays(30);

        ExplorerVineyardsCoupon = new(WellKnownUsers.Explorer, TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.AuthorId, 20, expiresOn, now);
        ExplorerAllToursCoupon = new(WellKnownUsers.Explorer, null, null, 10, expiresOn, now);
        SecondExplorerAllToursCoupon = new(WellKnownUsers.SecondExplorer, null, null, 15, expiresOn, now);
    }

    public static Coupon[] All => [ExplorerVineyardsCoupon, ExplorerAllToursCoupon, SecondExplorerAllToursCoupon];
}
