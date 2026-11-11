using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.FollowRequestSending;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequestSending;

public class FollowRequestSendingQueryTests : BaseIntegrationTest
{
    public FollowRequestSendingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Sent_requests_list_contains_only_requests_of_the_signed_in_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var explorersRequestIds = FollowRequestSeed.All
            .Where(followRequest => followRequest.SenderId == WellKnownUsers.Explorer)
            .Select(followRequest => followRequest.Id)
            .ToList();

        var sent = await client.GetFromJsonAsync<List<SentFollowRequestDto>>(
            "/api/social/follow-requests/sent", JsonOptions);

        sent.Should().OnlyContain(followRequest => explorersRequestIds.Contains(followRequest.Id));
        sent.Should().Contain(followRequest => followRequest.Id == FollowRequestSeed.ExplorerToSecondExplorerPending.Id);
        sent.Should().NotContain(followRequest => followRequest.Id == FollowRequestSeed.SecondExplorerToAdministratorPending.Id);
    }

    [Fact]
    public async Task Sent_requests_list_shows_only_the_latest_request_per_receiver_without_accepted_requests_or_ended_connections()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var sent = await client.GetFromJsonAsync<List<SentFollowRequestDto>>(
            "/api/social/follow-requests/sent", JsonOptions);

        sent.Should().NotContain(followRequest => followRequest.ReceiverId == FollowRequestSeed.FormerlyFollowedUser);
        sent.Should().NotContain(followRequest => followRequest.Id == FollowRequestSeed.ExplorerAcceptedByFollowedUser.Id);
        sent.Should().Contain(followRequest => followRequest.Id == FollowRequestSeed.ExplorerRejectedByRejectingUser.Id);
    }

    [Fact]
    public async Task Accepted_request_gives_following_state_without_a_pending_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var relationship = await client.GetFromJsonAsync<FollowRelationshipDto>(
            $"/api/social/follow-requests/relationship/{FollowRequestSeed.FollowedUser}", JsonOptions);

        relationship!.State.Should().Be(FollowRelationshipState.Following);
        relationship.PendingRequestId.Should().BeNull();
    }

    [Fact]
    public async Task Removed_follower_is_no_longer_following()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var relationship = await client.GetFromJsonAsync<FollowRelationshipDto>(
            $"/api/social/follow-requests/relationship/{WellKnownUsers.Explorer}", JsonOptions);

        relationship!.State.Should().Be(FollowRelationshipState.Follow);
    }

    [Fact]
    public async Task Pending_request_to_another_user_gives_pending_state_with_its_id()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var relationship = await client.GetFromJsonAsync<FollowRelationshipDto>(
            $"/api/social/follow-requests/relationship/{WellKnownUsers.SecondExplorer}", JsonOptions);

        relationship!.State.Should().Be(FollowRelationshipState.Pending);
        relationship.PendingRequestId.Should().Be(FollowRequestSeed.ExplorerToSecondExplorerPending.Id);
    }

    [Fact]
    public async Task Pending_request_from_another_user_does_not_give_pending_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var relationship = await client.GetFromJsonAsync<FollowRelationshipDto>(
            $"/api/social/follow-requests/relationship/{WellKnownUsers.Explorer}", JsonOptions);

        relationship!.State.Should().Be(FollowRelationshipState.Follow);
        relationship.PendingRequestId.Should().BeNull();
    }
}
