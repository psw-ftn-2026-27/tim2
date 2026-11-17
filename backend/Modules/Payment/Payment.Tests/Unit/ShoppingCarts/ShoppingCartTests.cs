using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class ShoppingCartTests
{
    [Fact]
    public void Adding_a_product_that_is_already_in_the_cart_is_rejected()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        var product = CreateProduct(WellKnownUsers.SecondExplorer, 500);
        cart.AddItem(product);

        var add = () => cart.AddItem(product);

        add.Should().Throw<DomainException>();
    }

    [Fact]
    public void Adding_a_product_owned_by_the_buyer_is_rejected()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        var ownProduct = CreateProduct(WellKnownUsers.Explorer, 500);

        var add = () => cart.AddItem(ownProduct);

        add.Should().Throw<DomainException>();
    }

    [Fact]
    public void Total_price_is_the_sum_of_item_prices_and_drops_when_an_item_is_removed()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        var cheap = CreateProduct(WellKnownUsers.SecondExplorer, 500);
        var expensive = CreateProduct(WellKnownUsers.SecondExplorer, 1200);
        cart.AddItem(cheap);
        cart.AddItem(expensive);
        var totalBefore = cart.TotalPrice();

        cart.RemoveItem(cheap.EntityId);

        totalBefore.Should().Be(1700);
        cart.TotalPrice().Should().Be(1200);
    }

    [Fact]
    public void Checkout_of_a_cart_with_a_tour_that_is_no_longer_published_is_rejected()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);
        var published = CreateProduct(WellKnownUsers.SecondExplorer, 500);
        var archived = CreateProduct(WellKnownUsers.SecondExplorer, 1200);
        cart.AddItem(published);
        cart.AddItem(archived);

        var checkout = () => cart.Checkout([published.EntityId]);

        checkout.Should().Throw<DomainException>();
    }

    [Fact]
    public void Checkout_of_an_empty_cart_is_rejected()
    {
        var cart = new ShoppingCart(WellKnownUsers.Explorer);

        var checkout = () => cart.Checkout([]);

        checkout.Should().Throw<DomainException>();
    }

    private static Product CreateProduct(Guid ownerId, int price)
    {
        return new Product(Guid.NewGuid(), "Tura", ownerId, price);
    }
}
