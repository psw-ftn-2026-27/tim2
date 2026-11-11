using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletViewing;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletViewing;

public class WalletViewingQueryTests : BaseIntegrationTest
{
    public WalletViewingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_the_balance_of_their_own_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var balance = await client.GetFromJsonAsync<WalletBalanceDto>("/api/payment/wallet/balance", JsonOptions);

        balance!.Balance.Should().Be(WalletSeed.ExplorersFundedWallet.Balance());
    }

    [Fact]
    public async Task Explorer_sees_only_their_own_transactions_newest_first()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var transactions = await client.GetFromJsonAsync<PageResult<TransactionDto>>("/api/payment/wallet/transactions", JsonOptions);

        transactions!.Items.Select(transaction => transaction.Id).Should()
            .BeEquivalentTo(WalletSeed.ExplorersFundedWallet.Transactions.Select(transaction => transaction.Id));
        transactions.Items.Should().BeInDescendingOrder(transaction => transaction.CreatedAt);
        transactions.TotalCount.Should().Be(WalletSeed.ExplorersFundedWallet.Transactions.Count);
    }

    [Fact]
    public async Task Explorer_without_a_wallet_has_a_zero_balance()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var balance = await client.GetFromJsonAsync<WalletBalanceDto>("/api/payment/wallet/balance", JsonOptions);

        balance!.Balance.Should().Be(0);
    }

    [Fact]
    public async Task Explorer_without_a_wallet_has_no_transactions()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var transactions = await client.GetFromJsonAsync<PageResult<TransactionDto>>("/api/payment/wallet/transactions", JsonOptions);

        transactions!.Items.Should().BeEmpty();
        transactions.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_a_wallet_balance()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallet/balance");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_a_wallet_balance()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/wallet/balance");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_wallet_transactions()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallet/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_wallet_transactions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/wallet/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
