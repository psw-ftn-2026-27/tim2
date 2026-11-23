using FluentAssertions;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Deposit_of_a_non_positive_amount_is_rejected(int amount)
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);

        var deposit = () => wallet.Deposit(WellKnownUsers.Administrator, amount);

        deposit.Should().Throw<DomainException>();
    }

    [Fact]
    public void Administrator_cancels_their_own_recent_deposit_and_the_balance_drops_by_its_amount()
    {
        var wallet = CreateWalletWithDeposits();
        var deposit = wallet.Transactions[1];
        var balanceBefore = wallet.Balance();

        wallet.CancelDeposit(deposit.Id, WellKnownUsers.Administrator, deposit.CreatedAt.AddHours(1));

        deposit.IsCancelled.Should().BeTrue();
        wallet.Balance().Should().Be(balanceBefore - deposit.Amount);
    }

    [Fact]
    public void Cancelling_another_administrators_deposit_is_rejected()
    {
        var wallet = CreateWalletWithDeposits();
        var deposit = wallet.Transactions[1];

        var cancel = () => wallet.CancelDeposit(deposit.Id, Guid.NewGuid(), deposit.CreatedAt.AddHours(1));

        cancel.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancelling_a_deposit_older_than_24_hours_is_rejected()
    {
        var wallet = CreateWalletWithDeposits();
        var deposit = wallet.Transactions[1];

        var cancel = () => wallet.CancelDeposit(deposit.Id, WellKnownUsers.Administrator, deposit.CreatedAt.AddHours(25));

        cancel.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancelling_a_deposit_younger_than_24_hours_is_allowed()
    {
        var wallet = CreateWalletWithDeposits();
        var deposit = wallet.Transactions[1];

        wallet.CancelDeposit(deposit.Id, WellKnownUsers.Administrator, deposit.CreatedAt.AddHours(23));

        deposit.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancelling_an_already_cancelled_deposit_is_rejected()
    {
        var wallet = CreateWalletWithDeposits();
        var deposit = wallet.Transactions[1];
        wallet.CancelDeposit(deposit.Id, WellKnownUsers.Administrator, deposit.CreatedAt.AddHours(1));

        var cancel = () => wallet.CancelDeposit(deposit.Id, WellKnownUsers.Administrator, deposit.CreatedAt.AddHours(2));

        cancel.Should().Throw<DomainException>();
    }

    [Fact]
    public void Purchase_without_enough_balance_is_rejected_and_the_balance_stays_the_same()
    {
        var wallet = CreateWalletWithDeposits();
        var balanceBefore = wallet.Balance();
        List<OrderItem> items = [new OrderItem(Guid.NewGuid(), "Tura", 100), new OrderItem(Guid.NewGuid(), "Druga tura", 51)];

        var purchase = () => wallet.Purchase(items);

        purchase.Should().Throw<DomainException>();
        wallet.Balance().Should().Be(balanceBefore);
    }

    [Fact]
    public void Purchase_adds_a_negative_transaction_for_each_item_and_lowers_the_balance_by_the_total()
    {
        var wallet = CreateWalletWithDeposits();
        var balanceBefore = wallet.Balance();
        List<OrderItem> items = [new OrderItem(Guid.NewGuid(), "Tura", 100), new OrderItem(Guid.NewGuid(), "Druga tura", 50)];

        wallet.Purchase(items);

        wallet.Transactions.Select(transaction => (transaction.EntityId, transaction.Name, transaction.Amount)).Should()
            .Contain(items.Select(item => ((Guid?)item.EntityId, (string?)item.Name, -item.Price)));
        wallet.Balance().Should().Be(balanceBefore - 150);
    }

    [Fact]
    public void Paying_a_gift_card_without_enough_balance_is_rejected_and_the_balance_stays_the_same()
    {
        var wallet = CreateWalletWithDeposits();
        var balanceBefore = wallet.Balance();

        var pay = () => wallet.PayGiftCard(Guid.NewGuid(), 151);

        pay.Should().Throw<DomainException>();
        wallet.Balance().Should().Be(balanceBefore);
    }

    [Fact]
    public void Paying_a_gift_card_adds_a_negative_transaction_and_lowers_the_balance_by_its_amount()
    {
        var wallet = CreateWalletWithDeposits();
        var balanceBefore = wallet.Balance();
        var giftCardId = Guid.NewGuid();

        wallet.PayGiftCard(giftCardId, 80);

        wallet.Transactions.Should().Contain(transaction => transaction.EntityId == giftCardId && transaction.Amount == -80);
        wallet.Balance().Should().Be(balanceBefore - 80);
    }

    private static Wallet CreateWalletWithDeposits()
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(WellKnownUsers.Administrator, 100);
        wallet.Deposit(WellKnownUsers.Administrator, 50);
        return wallet;
    }
}
