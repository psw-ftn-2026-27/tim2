using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration;

public sealed class PaymentApiTests : BaseIntegrationTest
{
    public PaymentApiTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_holding_a_purchase_token_for_the_tour_has_purchased_it()
    {
        var token = TourPurchaseTokenSeed.WealthyBuyersKalemegdan;
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(token.BuyerId, token.TourId);

        hasPurchased.Should().BeTrue();
    }

    [Fact]
    public async Task Buyer_has_not_purchased_a_tour_bought_by_another_buyer()
    {
        var othersToken = TourPurchaseTokenSeed.WealthyBuyersKalemegdan;
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(WalletSeed.BuyerOfArchivedTour, othersToken.TourId);

        hasPurchased.Should().BeFalse();
    }
}
