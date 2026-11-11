using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.FollowRequestResponding;
using Social.Domain.FollowRequests;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequestResponding;

public class FollowRequestRespondingQueryTests : BaseIntegrationTest
{
    public FollowRequestRespondingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Received_requests_list_contains_only_pending_requests_of_the_signed_in_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expectedIds = FollowRequestSeed.All
            .Where(followRequest =>
                followRequest.ReceiverId == WellKnownUsers.Explorer &&
                followRequest.Status == FollowRequestStatus.Pending)
            .Select(followRequest => followRequest.Id)
            .ToList();

        var received = await client.GetFromJsonAsync<List<ReceivedFollowRequestDto>>(
            "/api/social/follow-requests/received", JsonOptions);

        received!.Select(followRequest => followRequest.Id).Should().BeEquivalentTo(expectedIds);
        received.Should().Contain(followRequest =>
            followRequest.Id == FollowRequestSeed.RequestingUserToExplorerPending.Id &&
            followRequest.SenderId == FollowRequestSeed.RequestingUser);
        received.Should().NotContain(followRequest => followRequest.Id == FollowRequestSeed.AdministratorRemovedByExplorer.Id);
        received.Should().NotContain(followRequest => followRequest.Id == FollowRequestSeed.ExplorerToSecondExplorerPending.Id);
    }
}
