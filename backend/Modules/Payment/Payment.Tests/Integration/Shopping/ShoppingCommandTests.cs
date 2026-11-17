using System.Net;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Shopping;

public class ShoppingCommandTests : BaseIntegrationTest
{
    public ShoppingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Adding_a_published_tour_stores_an_item_with_its_name_and_price_and_creates_its_product()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedExpensiveMountain;

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{tour.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var item = assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().ContainSingle(item => item.EntityId == tour.Id).Subject;
        item.Name.Should().Be(tour.Name);
        item.Price.Should().Be(tour.Price);
        var product = assertContext.Products.Single(product => product.EntityId == tour.Id);
        product.Name.Should().Be(tour.Name);
        product.OwnerId.Should().Be(tour.AuthorId);
        product.Price.Should().Be(tour.Price);
    }

    [Fact]
    public async Task Adding_a_tour_for_a_buyer_without_a_cart_creates_their_cart_with_that_item()
    {
        var buyerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(buyerId, "explorer");
        var tourId = TourSeed.PublishedExpensiveMountain.Id;

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{tourId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == buyerId)
            .Items.Should().ContainSingle(item => item.EntityId == tourId);
    }

    [Fact]
    public async Task Adding_an_own_tour_is_rejected_and_the_cart_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.PublishedVineyards.Id;
        int itemCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            itemCount = arrangeContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer).Items.Count;
        }

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{tourId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().HaveCount(itemCount);
    }

    [Fact]
    public async Task Adding_a_tour_whose_product_already_exists_creates_no_new_product()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = ProductSeed.FreeParkWalkInNoCart.EntityId;
        int productCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            productCount = arrangeContext.Products.Count();
        }

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{tourId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCount);
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().Contain(item => item.EntityId == tourId);
    }

    [Fact]
    public async Task Adding_an_unpublished_tour_is_rejected_and_the_cart_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = TourSeed.SecondExplorersTrail.Id;
        int itemCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            itemCount = arrangeContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer).Items.Count;
        }

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{tourId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().HaveCount(itemCount);
        assertContext.Products.Any(product => product.EntityId == tourId).Should().BeFalse();
    }

    [Fact]
    public async Task Adding_a_tour_that_is_already_in_the_cart_is_rejected_and_the_cart_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = ProductSeed.CheapGallery.EntityId;
        int itemCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            itemCount = arrangeContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer).Items.Count;
        }

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{tourId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().HaveCount(itemCount);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_to_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{TourSeed.PublishedExpensiveMountain.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_to_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/shopping/cart/items/{TourSeed.PublishedExpensiveMountain.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Removing_an_item_deletes_it_from_the_stored_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tourId = ProductSeed.CheapGallery.EntityId;

        var response = await client.DeleteAsync($"/api/payment/shopping/cart/items/{tourId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().NotContain(item => item.EntityId == tourId);
    }

    [Fact]
    public async Task Removing_from_a_cart_that_does_not_exist_is_rejected_and_creates_no_cart()
    {
        var buyerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(buyerId, "explorer");

        var response = await client.DeleteAsync($"/api/payment/shopping/cart/items/{ProductSeed.CheapGallery.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Any(cart => cart.BuyerId == buyerId).Should().BeFalse();
    }

    [Fact]
    public async Task Removing_a_tour_that_is_not_in_the_cart_is_rejected_and_the_cart_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        int itemCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            itemCount = arrangeContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer).Items.Count;
        }

        var response = await client.DeleteAsync($"/api/payment/shopping/cart/items/{ProductSeed.Vineyards.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.BuyerId == WellKnownUsers.Explorer)
            .Items.Should().HaveCount(itemCount);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_from_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/payment/shopping/cart/items/{ProductSeed.CheapGallery.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_from_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/payment/shopping/cart/items/{ProductSeed.CheapGallery.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

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
