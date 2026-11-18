using System.Net.Http.Json;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Application.CouponManagement;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponManagement;

public class CouponManagementQueryTests : BaseIntegrationTest
{
    public CouponManagementQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task My_coupons_return_the_coupons_of_the_owner_with_tour_names_and_not_other_peoples()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var coupons = (await client.GetFromJsonAsync<List<CouponDto>>("/api/payment/coupons", JsonOptions))!;

        coupons.Select(coupon => coupon.Id).Should().Contain([CouponSeed.ExplorerVineyardsCoupon.Id, CouponSeed.ExplorerAllToursCoupon.Id]);
        coupons.Select(coupon => coupon.Id).Should().NotContain(CouponSeed.SecondExplorerAllToursCoupon.Id);
        var vineyards = coupons.Single(coupon => coupon.Id == CouponSeed.ExplorerVineyardsCoupon.Id);
        vineyards.Code.Should().Be(CouponSeed.ExplorerVineyardsCoupon.Code);
        vineyards.ProductName.Should().Be(TourSeed.PublishedVineyards.Name);
        coupons.Single(coupon => coupon.Id == CouponSeed.ExplorerAllToursCoupon.Id).ProductId.Should().BeNull();
    }
}
