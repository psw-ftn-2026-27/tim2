using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Connections;
using Social.Application.FollowRequests;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Connections;

public class ConnectionsQueryTests : BaseIntegrationTest
{
    public ConnectionsQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Followers_are_shown_with_the_relationship_from_the_signed_in_user_to_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>(
            "/api/social/connections/followers", JsonOptions);

        followers!.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.FollowedUser, FollowRelationshipState.Following, null));
        followers.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.PendingFollower, FollowRelationshipState.Pending,
                FollowRequestSeed.ExplorerToPendingFollowerPending.Id));
        followers.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.NotFollowedFollower, FollowRelationshipState.Follow, null));
    }

    [Fact]
    public async Task Removed_follower_is_not_among_the_followers_of_the_receiver()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>(
            "/api/social/connections/followers", JsonOptions);

        followers!.Items.Should().NotContain(follower => follower.UserId == WellKnownUsers.Administrator);
        followers.Items.Should().Contain(follower => follower.UserId == FollowRequestSeed.NotFollowedFollower);
    }

    [Fact]
    public async Task Receiver_who_removed_a_follower_is_not_among_the_users_that_follower_follows()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var following = await client.GetFromJsonAsync<PageResult<FollowedUserDto>>(
            "/api/social/connections/following", JsonOptions);

        following!.Items.Should().NotContain(followed => followed.UserId == FollowRequestSeed.FormerlyFollowedUser);
        following.Items.Should().Contain(followed => followed.UserId == FollowRequestSeed.FollowedUser);
    }
}
