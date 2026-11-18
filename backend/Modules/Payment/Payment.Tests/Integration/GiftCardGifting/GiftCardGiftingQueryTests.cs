using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardGifting;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCardGifting;

public class GiftCardGiftingQueryTests : BaseIntegrationTest
{
    public GiftCardGiftingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Overview_returns_the_cards_the_user_sent_and_received_and_not_other_peoples()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var overview = await client.GetFromJsonAsync<GiftCardOverviewDto>("/api/payment/gift-cards", JsonOptions);

        overview!.Sent.Select(card => card.Id).Should().Contain(GiftCardSeed.ExplorerToSecondExplorer.Id);
        overview.Received.Select(card => card.Id).Should().Contain(GiftCardSeed.SecondExplorerToExplorer.Id);
        overview.Sent.Select(card => card.Id).Should().NotContain(GiftCardSeed.SecondExplorerToThirdExplorer.Id);
        overview.Received.Select(card => card.Id).Should().NotContain(GiftCardSeed.SecondExplorerToThirdExplorer.Id);
        overview.Sent.Should().OnlyContain(card => card.OtherPartyEmail == "second-explorer@test.com");
    }
}
