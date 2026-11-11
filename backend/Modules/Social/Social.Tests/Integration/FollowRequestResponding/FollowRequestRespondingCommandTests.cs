using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.FollowRequests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequestResponding;

public class FollowRequestRespondingCommandTests : BaseIntegrationTest
{
    public FollowRequestRespondingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Accepted_request_makes_the_sender_follow_the_receiver_but_not_the_other_way_around()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Any(followRequest =>
                followRequest.SenderId == WellKnownUsers.Explorer &&
                followRequest.ReceiverId == WellKnownUsers.SecondExplorer &&
                followRequest.Status == FollowRequestStatus.Accepted)
            .Should().BeTrue();
        assertContext.FollowRequests
            .Any(followRequest =>
                followRequest.SenderId == WellKnownUsers.SecondExplorer &&
                followRequest.ReceiverId == WellKnownUsers.Explorer &&
                followRequest.Status == FollowRequestStatus.Accepted)
            .Should().BeFalse();
    }

    [Fact]
    public async Task Request_that_is_no_longer_pending_cannot_be_accepted_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.AdministratorRemovedByExplorer.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Single(followRequest => followRequest.Id == FollowRequestSeed.AdministratorRemovedByExplorer.Id)
            .Status.Should().Be(FollowRequestStatus.Removed);
    }

    [Fact]
    public async Task User_who_is_not_the_receiver_cannot_accept_a_request_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Single(followRequest => followRequest.Id == FollowRequestSeed.ExplorerToSecondExplorerPending.Id)
            .Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rejected_request_is_stored_as_rejected_and_no_following_is_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Single(followRequest => followRequest.Id == FollowRequestSeed.ExplorerToSecondExplorerPending.Id)
            .Status.Should().Be(FollowRequestStatus.Rejected);
        assertContext.FollowRequests
            .Any(followRequest =>
                followRequest.SenderId == WellKnownUsers.Explorer &&
                followRequest.ReceiverId == WellKnownUsers.SecondExplorer &&
                followRequest.Status == FollowRequestStatus.Accepted)
            .Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{FollowRequestSeed.ExplorerToSecondExplorerPending.Id}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
