using System.Net;
using System.Net.Http.Json;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Application.Shopping;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Shopping;

public class ShoppingQueryTests : BaseIntegrationTest
{
    public ShoppingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_only_their_own_items_and_their_total()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seededCart = ShoppingCartSeed.ExplorersCartWithGalleryAndFortress;

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/shopping/cart", JsonOptions);

        cart!.Items.Select(item => (item.TourId, item.Name, item.Price)).Should()
            .BeEquivalentTo(seededCart.Items.Select(item => (item.EntityId, item.Name, item.Price)));
        cart.TotalPrice.Should().Be(seededCart.TotalPrice());
    }

    [Fact]
    public async Task Explorer_sees_the_name_and_price_of_a_published_tour_on_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedExpensiveMountain;

        var offer = await client.GetFromJsonAsync<TourOfferDto>($"/api/payment/shopping/tours/{tour.Id}", JsonOptions);

        offer.Should().BeEquivalentTo(new TourOfferDto(tour.Id, tour.Name, tour.Price));
    }

    [Fact]
    public async Task A_tour_that_is_not_published_is_not_on_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/shopping/tours/{TourSeed.SecondExplorersTrail.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/shopping/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/shopping/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
