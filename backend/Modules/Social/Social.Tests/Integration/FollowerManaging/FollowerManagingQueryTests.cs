using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.FollowerManaging;
using Social.Application.FollowRequests;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowerManaging;

public class FollowerManagingQueryTests : BaseIntegrationTest
{
    public FollowerManagingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Followers_are_shown_with_the_relationship_from_the_signed_in_user_to_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>(
            "/api/social/follow-requests/followers", JsonOptions);

        followers!.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.FollowedUser, FollowRelationshipState.Following,
                FollowRequestSeed.FollowedUserAcceptedByExplorer.Id, null));
        followers.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.PendingFollower, FollowRelationshipState.Pending,
                FollowRequestSeed.PendingFollowerAcceptedByExplorer.Id,
                FollowRequestSeed.ExplorerToPendingFollowerPending.Id));
        followers.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.NotFollowedFollower, FollowRelationshipState.Follow,
                FollowRequestSeed.NotFollowedFollowerAcceptedByExplorer.Id, null));
    }

    [Fact]
    public async Task Removed_follower_is_not_among_the_followers_of_the_receiver()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>(
            "/api/social/follow-requests/followers", JsonOptions);

        followers!.Items.Should().NotContain(follower => follower.UserId == WellKnownUsers.Administrator);
        followers.Items.Should().Contain(follower => follower.UserId == FollowRequestSeed.NotFollowedFollower);
    }

    [Fact]
    public async Task Receiver_who_removed_a_follower_is_not_among_the_users_that_follower_follows()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var following = await client.GetFromJsonAsync<PageResult<FollowedUserDto>>(
            "/api/social/follow-requests/following", JsonOptions);

        following!.Items.Should().NotContain(followed => followed.UserId == FollowRequestSeed.FormerlyFollowedUser);
        following.Items.Should().Contain(followed => followed.UserId == FollowRequestSeed.FollowedUser);
    }

    [Fact]
    public async Task Receiver_who_removed_a_follower_stays_among_the_followers_of_that_follower_with_follow_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>(
            "/api/social/follow-requests/followers", JsonOptions);

        followers!.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.FormerlyFollowedUser, FollowRelationshipState.Follow,
                FollowRequestSeed.FormerlyFollowedUserAcceptedByExplorer.Id, null));
    }

    [Fact]
    public async Task Unfollowed_user_is_not_among_the_followed_users()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var following = await client.GetFromJsonAsync<PageResult<FollowedUserDto>>(
            "/api/social/follow-requests/following", JsonOptions);

        following!.Items.Should().NotContain(followed => followed.UserId == FollowRequestSeed.UnfollowedUser);
        following.Items.Should().Contain(followed => followed.UserId == FollowRequestSeed.FollowedUser);
    }

    [Fact]
    public async Task Unfollowed_user_stays_among_the_followers_with_follow_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>(
            "/api/social/follow-requests/followers", JsonOptions);

        followers!.Items.Should().ContainEquivalentOf(
            new FollowerDto(FollowRequestSeed.UnfollowedUser, FollowRelationshipState.Follow,
                FollowRequestSeed.UnfollowedUserAcceptedByExplorer.Id, null));
    }
}
