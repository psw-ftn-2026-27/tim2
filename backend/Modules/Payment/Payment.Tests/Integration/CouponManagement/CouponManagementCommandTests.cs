using System.Net;
using System.Net.Http.Json;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Application.CouponManagement;
using Payment.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponManagement;

public class CouponManagementCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/payment/coupons";

    public CouponManagementCommandTests(PaymentApiFactory factory) : base(factory) { }

    private static DateOnly InThirtyDays() => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);

    [Fact]
    public async Task Creating_a_coupon_for_an_own_published_tour_saves_it_with_all_fields()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyards;
        var expiresOn = InThirtyDays();
        var dto = new CreateCouponDto(tour.Id, 25, expiresOn);
        int couponCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            couponCount = arrangeContext.Coupons.Count();
        }

        var response = await client.PostAsJsonAsync(Url, dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var couponId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponCount + 1);
        var coupon = assertContext.Coupons.Single(coupon => coupon.Id == couponId);
        coupon.OwnerId.Should().Be(WellKnownUsers.Explorer);
        coupon.ProductEntityId.Should().Be(tour.Id);
        coupon.Percent.Should().Be(25);
        coupon.ExpiresOn.Should().Be(expiresOn);
        coupon.Code.Should().HaveLength(8);
    }

    [Fact]
    public async Task Creating_a_coupon_for_an_unpublished_tour_is_rejected_and_nothing_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var dto = new CreateCouponDto(TourSeed.DraftOldTownWithThreeKeyPoints.Id, 25, InThirtyDays());
        int couponCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            couponCount = arrangeContext.Coupons.Count();
        }

        var response = await client.PostAsJsonAsync(Url, dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponCount);
    }

    [Fact]
    public async Task Creating_a_coupon_for_another_authors_tour_is_rejected_and_nothing_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var dto = new CreateCouponDto(TourSeed.PublishedLakeside.Id, 25, InThirtyDays());
        int couponCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            couponCount = arrangeContext.Coupons.Count();
        }

        var response = await client.PostAsJsonAsync(Url, dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponCount);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_coupon()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(Url, new CreateCouponDto(null, 25, InThirtyDays()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_coupon()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(Url, new CreateCouponDto(null, 25, InThirtyDays()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
