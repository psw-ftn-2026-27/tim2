using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletFunding;
using Payment.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletFunding;

public class WalletFundingCommandTests : BaseIntegrationTest
{
    public WalletFundingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Deposit_to_an_explorer_without_a_wallet_creates_a_wallet_holding_the_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/users/{WellKnownUsers.SecondExplorer}/deposits", new DepositDto(75));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var wallet = assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.SecondExplorer);
        var transaction = wallet.Transactions.Should().ContainSingle().Subject;
        transaction.AdminId.Should().Be(WellKnownUsers.Administrator);
        transaction.Amount.Should().Be(75);
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Deposit_of_zero_is_rejected_and_writes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        int transactionCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            transactionCount = arrangeContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Transactions.Count;
        }

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/users/{WellKnownUsers.Explorer}/deposits", new DepositDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer).Transactions.Should().HaveCount(transactionCount);
    }

    [Fact]
    public async Task Deposit_to_an_unknown_user_is_rejected_and_creates_no_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var unknownUserId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/users/{unknownUserId}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Any(wallet => wallet.UserId == unknownUserId).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_deposit()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/users/{WellKnownUsers.Explorer}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/users/{WellKnownUsers.Explorer}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
