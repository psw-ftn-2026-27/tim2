using FluentAssertions;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardTests
{
    [Fact]
    public void Gift_card_cannot_be_created_for_the_sender_themselves()
    {
        var create = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.Explorer, 50, "Poruka", DateTime.UtcNow);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(9)]
    [InlineData(0)]
    [InlineData(1001)]
    public void Gift_card_with_an_amount_outside_10_to_1000_is_rejected(int amount)
    {
        var create = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, amount, "Poruka", DateTime.UtcNow);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public void Gift_card_with_an_amount_on_the_edge_of_the_range_is_created(int amount)
    {
        var giftCard = new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, amount, "Poruka", DateTime.UtcNow);

        giftCard.Amount.Should().Be(amount);
    }

    [Fact]
    public void Gift_card_with_a_message_longer_than_200_characters_is_rejected()
    {
        var create = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 50, new string('a', 201), DateTime.UtcNow);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_expires_three_years_after_creation()
    {
        var createdAt = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc);

        var giftCard = new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 50, "Poruka", createdAt);

        giftCard.ExpiresAt.Should().Be(createdAt.AddYears(3));
    }

    [Fact]
    public void Gift_card_is_valid_until_its_expiry_and_expired_after_it()
    {
        var giftCard = new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 50, "Poruka", DateTime.UtcNow);

        var beforeExpiry = giftCard.CurrentStatus(giftCard.ExpiresAt.AddDays(-1));
        var afterExpiry = giftCard.CurrentStatus(giftCard.ExpiresAt.AddDays(1));

        beforeExpiry.Should().Be(GiftCardStatus.Valid);
        afterExpiry.Should().Be(GiftCardStatus.Expired);
    }
}
