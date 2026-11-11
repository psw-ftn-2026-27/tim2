using FluentAssertions;
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
}
