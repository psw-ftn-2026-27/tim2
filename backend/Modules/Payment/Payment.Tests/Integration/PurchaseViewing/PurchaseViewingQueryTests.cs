using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.PurchaseViewing;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.PurchaseViewing;

public class PurchaseViewingQueryTests : BaseIntegrationTest
{
    public PurchaseViewingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_only_their_own_purchased_tours_newest_first()
    {
        var client = Factory.CreateClientFor(WalletSeed.WealthyBuyer, "explorer");
        var seededTokens = TourPurchaseTokenSeed.All.Where(token => token.BuyerId == WalletSeed.WealthyBuyer).ToList();

        var tours = await client.GetFromJsonAsync<PageResult<PurchasedTourDto>>("/api/payment/purchases/tours", JsonOptions);

        tours!.Items.Select(tour => (tour.TourId, tour.Name, tour.Price)).Should()
            .BeEquivalentTo(seededTokens.Select(token => (token.TourId, token.Name, token.Price)));
        tours.Items.Should().BeInDescendingOrder(tour => tour.PurchasedAt);
        tours.TotalCount.Should().Be(seededTokens.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_purchased_tours()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/purchases/tours");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_purchased_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/purchases/tours");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
