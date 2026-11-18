using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardGifting;
using Payment.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCardGifting;

public class GiftCardGiftingCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/payment/gift-cards";
    private const string SecondExplorerEmail = "second-explorer@test.com";

    public GiftCardGiftingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Purchasing_a_gift_card_saves_it_for_the_recipient_and_lowers_the_senders_balance_by_its_amount()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var dto = new PurchaseGiftCardDto(SecondExplorerEmail, 40, "Uzivaj");
        int balance;
        int cardCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            balance = arrangeContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Balance();
            cardCount = arrangeContext.GiftCards.Count();
        }

        var response = await client.PostAsJsonAsync(Url, dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var giftCardId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(cardCount + 1);
        var giftCard = assertContext.GiftCards.Single(card => card.Id == giftCardId);
        giftCard.SenderId.Should().Be(WellKnownUsers.Explorer);
        giftCard.RecipientId.Should().Be(WellKnownUsers.SecondExplorer);
        giftCard.Amount.Should().Be(40);
        assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer)
            .Balance().Should().Be(balance - 40);
    }

    [Fact]
    public async Task Purchasing_a_gift_card_without_enough_EC_is_rejected_and_no_card_or_wallet_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var dto = new PurchaseGiftCardDto("explorer@test.com", 40, "Uzivaj");
        int cardCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            cardCount = arrangeContext.GiftCards.Count();
        }

        var response = await client.PostAsJsonAsync(Url, dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(cardCount);
        assertContext.Wallets.Any(wallet => wallet.UserId == WellKnownUsers.SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public async Task Purchasing_a_gift_card_for_a_nonexistent_recipient_is_rejected_and_nothing_is_saved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var dto = new PurchaseGiftCardDto("nobody@test.com", 40, "Uzivaj");
        int balance;
        int cardCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            balance = arrangeContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Balance();
            cardCount = arrangeContext.GiftCards.Count();
        }

        var response = await client.PostAsJsonAsync(Url, dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(cardCount);
        assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Balance().Should().Be(balance);
    }

    [Fact]
    public async Task Anonymous_user_cannot_purchase_a_gift_card()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(Url, new PurchaseGiftCardDto(SecondExplorerEmail, 40, "Uzivaj"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_purchase_a_gift_card()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(Url, new PurchaseGiftCardDto(SecondExplorerEmail, 40, "Uzivaj"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
