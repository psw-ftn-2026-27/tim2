using FluentAssertions;
using Identity.Contracts;
using Identity.Tests.Integration.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityApiTests : BaseIntegrationTest
{
    public IdentityApiTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorers_are_listed_with_their_email_and_administrators_are_not()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorers = await identityApi.GetExplorersAsync(1, 100);

        explorers.Items.Should().BeEquivalentTo(
            UserSeed.Explorers.Select(user => new ExplorerSummary(user.Id, user.Email!)));
        explorers.Items.Should().NotContain(explorer => explorer.UserId == UserSeed.Administrator.Id);
        explorers.TotalCount.Should().Be(UserSeed.Explorers.Length);
    }

    [Fact]
    public async Task A_page_holds_at_most_the_requested_number_of_explorers()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorers = await identityApi.GetExplorersAsync(2, 1);

        explorers.Items.Should().ContainSingle().Which.Should().Be(
            new ExplorerSummary(UserSeed.SecondExplorer.Id, UserSeed.SecondExplorer.Email!));
        explorers.TotalCount.Should().Be(UserSeed.Explorers.Length);
    }

    [Fact]
    public async Task Explorer_is_found_by_identifier()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorer = await identityApi.GetExplorerAsync(UserSeed.Explorer.Id);

        explorer.Should().Be(new ExplorerSummary(UserSeed.Explorer.Id, UserSeed.Explorer.Email!));
    }

    [Fact]
    public async Task Unknown_user_is_not_found()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorer = await identityApi.GetExplorerAsync(Guid.NewGuid());

        explorer.Should().BeNull();
    }

    [Fact]
    public async Task Administrator_is_not_found_as_an_explorer()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var explorer = await identityApi.GetExplorerAsync(UserSeed.Administrator.Id);

        explorer.Should().BeNull();
    }
}
