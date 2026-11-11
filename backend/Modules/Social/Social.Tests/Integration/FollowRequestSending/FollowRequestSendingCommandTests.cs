using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.FollowRequestSending;
using Social.Domain.FollowRequests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequestSending;

public class FollowRequestSendingCommandTests : BaseIntegrationTest
{
    public FollowRequestSendingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Sent_request_is_stored_as_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var receiverId = Guid.NewGuid();
        var request = new SendFollowRequestDto(receiverId);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.FollowRequests.Count();

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Count().Should().Be(requestCountBefore + 1);
        var stored = assertContext.FollowRequests.Single(followRequest => followRequest.ReceiverId == receiverId);
        stored.SenderId.Should().Be(WellKnownUsers.Explorer);
        stored.Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Second_request_while_the_first_is_pending_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new SendFollowRequestDto(WellKnownUsers.SecondExplorer);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.FollowRequests.Count();

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Count().Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_request()
    {
        var client = Factory.CreateClient();
        var request = new SendFollowRequestDto(WellKnownUsers.SecondExplorer);

        var response = await client.PostAsJsonAsync("/api/social/follow-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Withdrawn_request_is_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Any(followRequest => followRequest.Id == FollowRequestSeed.ExplorerToSecondExplorerPending.Id)
            .Should().BeFalse();
    }

    [Fact]
    public async Task Withdrawing_a_nonexistent_request_is_not_found_and_nothing_is_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.FollowRequests.Count();

        var response = await client.DeleteAsync($"/api/social/follow-requests/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Count().Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
