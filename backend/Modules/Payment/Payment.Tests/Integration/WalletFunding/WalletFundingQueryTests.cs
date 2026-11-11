using System.Net;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletFunding;

public class WalletFundingQueryTests : BaseIntegrationTest
{
    public WalletFundingQueryTests(PaymentApiFactory factory) : base(factory) { }

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
