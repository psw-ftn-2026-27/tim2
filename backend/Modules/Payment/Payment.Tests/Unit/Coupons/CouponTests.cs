using FluentAssertions;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponTests
{
    private static readonly DateTime Now = new(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Tomorrow = new(2026, 5, 11);

    [Fact]
    public void Coupon_cannot_be_bound_to_a_product_owned_by_someone_else()
    {
        var create = () => new Coupon(WellKnownUsers.Explorer, Guid.NewGuid(), WellKnownUsers.SecondExplorer, 20, Tomorrow, Now);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Coupon_without_a_product_is_created_for_all_products_of_its_owner()
    {
        var coupon = new Coupon(WellKnownUsers.Explorer, null, null, 20, Tomorrow, Now);

        coupon.ProductEntityId.Should().BeNull();
        coupon.OwnerId.Should().Be(WellKnownUsers.Explorer);
    }

    [Theory]
    [InlineData(2026, 5, 10)]
    [InlineData(2026, 5, 9)]
    public void Coupon_that_does_not_expire_after_today_is_rejected(int year, int month, int day)
    {
        var create = () => new Coupon(WellKnownUsers.Explorer, null, null, 20, new DateOnly(year, month, day), Now);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Coupon_with_a_percent_outside_1_to_100_is_rejected(int percent)
    {
        var create = () => new Coupon(WellKnownUsers.Explorer, null, null, percent, Tomorrow, Now);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Coupon_with_a_percent_on_the_edge_of_the_range_is_created(int percent)
    {
        var coupon = new Coupon(WellKnownUsers.Explorer, null, null, percent, Tomorrow, Now);

        coupon.Percent.Should().Be(percent);
    }

    [Fact]
    public void Coupon_code_has_8_characters()
    {
        var coupon = new Coupon(WellKnownUsers.Explorer, null, null, 20, Tomorrow, Now);

        coupon.Code.Should().HaveLength(8);
    }
}
