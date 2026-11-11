using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.FollowRequests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowerManaging;

public class FollowerManagingCommandTests : BaseIntegrationTest
{
    public FollowerManagingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Removed_follower_is_stored_as_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/followers/{FollowRequestSeed.NotFollowedFollowerAcceptedByExplorer.Id}/remove", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Single(followRequest => followRequest.Id == FollowRequestSeed.NotFollowedFollowerAcceptedByExplorer.Id)
            .Status.Should().Be(FollowRequestStatus.Removed);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_follower()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/follow-requests/followers/{FollowRequestSeed.NotFollowedFollowerAcceptedByExplorer.Id}/remove", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unfollowed_connection_is_stored_as_unfollowed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/following/{FollowRequestSeed.ExplorerAcceptedByFollowedUser.Id}/unfollow", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Single(followRequest => followRequest.Id == FollowRequestSeed.ExplorerAcceptedByFollowedUser.Id)
            .Status.Should().Be(FollowRequestStatus.Unfollowed);
    }

    [Fact]
    public async Task User_who_is_not_the_sender_cannot_unfollow_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(FollowRequestSeed.FollowedUser, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/following/{FollowRequestSeed.ExplorerAcceptedByFollowedUser.Id}/unfollow", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests
            .Single(followRequest => followRequest.Id == FollowRequestSeed.ExplorerAcceptedByFollowedUser.Id)
            .Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task Anonymous_user_cannot_unfollow()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/follow-requests/following/{FollowRequestSeed.ExplorerAcceptedByFollowedUser.Id}/unfollow", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
