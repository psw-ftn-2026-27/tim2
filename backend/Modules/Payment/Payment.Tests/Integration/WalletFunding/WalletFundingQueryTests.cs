using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletFunding;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletFunding;

public class WalletFundingQueryTests : BaseIntegrationTest
{
    public WalletFundingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_each_explorers_balance_and_zero_for_one_without_a_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var wallets = await client.GetFromJsonAsync<PageResult<UserWalletDto>>("/api/payment/wallet/users", JsonOptions);

        wallets!.Items.Should().ContainSingle(wallet => wallet.UserId == WellKnownUsers.Explorer)
            .Which.Balance.Should().Be(WalletSeed.ExplorersFundedWallet.Balance());
        wallets.Items.Should().ContainSingle(wallet => wallet.UserId == WellKnownUsers.SecondExplorer)
            .Which.Balance.Should().Be(0);
        wallets.Items.Should().NotContain(wallet => wallet.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_user_wallets()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallet/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_see_user_wallets()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync("/api/payment/wallet/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
