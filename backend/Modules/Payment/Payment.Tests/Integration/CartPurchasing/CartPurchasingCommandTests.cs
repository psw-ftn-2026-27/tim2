using System.Net;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CartPurchasing;

public class CartPurchasingCommandTests : BaseIntegrationTest
{
    public CartPurchasingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Purchasing_the_cart_empties_it_charges_the_wallet_and_creates_a_token_for_each_item()
    {
        var client = Factory.CreateClientFor(WalletSeed.WealthyBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WealthyBuyersCartWithGalleryAndFortress;
        int balance;
        int tokenCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            balance = arrangeContext.Wallets.Single(wallet => wallet.UserId == WalletSeed.WealthyBuyer).Balance();
            tokenCount = arrangeContext.TourPurchaseTokens.Count(token => token.BuyerId == WalletSeed.WealthyBuyer);
        }

        var response = await client.PostAsync("/api/payment/shopping/cart/purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WalletSeed.WealthyBuyer).Items.Should().BeEmpty();
        assertContext.Wallets.Single(wallet => wallet.UserId == WalletSeed.WealthyBuyer)
            .Balance().Should().Be(balance - seededCart.TotalPrice());
        var tokens = assertContext.TourPurchaseTokens.Where(token => token.BuyerId == WalletSeed.WealthyBuyer).ToList();
        tokens.Should().HaveCount(tokenCount + seededCart.Items.Count);
        tokens.Select(token => token.TourId).Should().Contain(seededCart.Items.Select(item => item.EntityId));
    }

    [Fact]
    public async Task Purchasing_without_enough_EC_is_rejected_and_the_cart_the_balance_and_the_tokens_stay_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        int itemCount;
        int balance;
        int tokenCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            itemCount = arrangeContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer).Items.Count;
            balance = arrangeContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Balance();
            tokenCount = arrangeContext.TourPurchaseTokens.Count();
        }

        var response = await client.PostAsync("/api/payment/shopping/cart/purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer).Items.Should().HaveCount(itemCount);
        assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Balance().Should().Be(balance);
        assertContext.TourPurchaseTokens.Count().Should().Be(tokenCount);
    }

    [Fact]
    public async Task Purchasing_a_cart_with_a_tour_archived_in_the_meantime_is_rejected_and_nothing_is_written()
    {
        var client = Factory.CreateClientFor(WalletSeed.BuyerOfArchivedTour, "explorer");
        int itemCount;
        int balance;
        int tokenCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            itemCount = arrangeContext.ShoppingCarts.Single(cart => cart.BuyerId == WalletSeed.BuyerOfArchivedTour).Items.Count;
            balance = arrangeContext.Wallets.Single(wallet => wallet.UserId == WalletSeed.BuyerOfArchivedTour).Balance();
            tokenCount = arrangeContext.TourPurchaseTokens.Count();
        }

        var response = await client.PostAsync("/api/payment/shopping/cart/purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WalletSeed.BuyerOfArchivedTour).Items.Should().HaveCount(itemCount);
        assertContext.Wallets.Single(wallet => wallet.UserId == WalletSeed.BuyerOfArchivedTour).Balance().Should().Be(balance);
        assertContext.TourPurchaseTokens.Count().Should().Be(tokenCount);
    }

    [Fact]
    public async Task Purchasing_without_a_cart_is_rejected_and_creates_no_wallet()
    {
        var buyerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(buyerId, "explorer");

        var response = await client.PostAsync("/api/payment/shopping/cart/purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Any(wallet => wallet.UserId == buyerId).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_purchase_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/shopping/cart/purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_purchase_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/payment/shopping/cart/purchase", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
