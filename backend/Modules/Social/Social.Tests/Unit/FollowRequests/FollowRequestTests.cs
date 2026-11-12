using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.FollowRequests;
using Xunit;

namespace Social.Tests.Unit.FollowRequests;

public class FollowRequestTests
{
    private static readonly Guid Sender = WellKnownUsers.Explorer;
    private static readonly Guid Receiver = WellKnownUsers.SecondExplorer;
    private static readonly DateTime SentAt = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private static FollowRequest CreatePendingRequest() => new(Sender, Receiver, SentAt);

    private static FollowRequest CreateAcceptedRequest()
    {
        var request = CreatePendingRequest();
        request.AcceptAutomatically();
        return request;
    }

    private static FollowRequest CreateRejectedRequest()
    {
        var request = CreatePendingRequest();
        request.Reject(Receiver);
        return request;
    }

    private static FollowRequest CreateRemovedRequest()
    {
        var request = CreateAcceptedRequest();
        request.RemoveFollower(Receiver);
        return request;
    }

    private static FollowRequest CreateUnfollowedRequest()
    {
        var request = CreateAcceptedRequest();
        request.Unfollow(Sender);
        return request;
    }

    private static FollowRequest CreateRequestWithStatus(FollowRequestStatus status)
    {
        return status switch
        {
            FollowRequestStatus.Pending => CreatePendingRequest(),
            FollowRequestStatus.Removed => CreateRemovedRequest(),
            FollowRequestStatus.Unfollowed => CreateUnfollowedRequest(),
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }

    [Fact]
    public void User_cannot_send_a_request_to_themselves()
    {
        var creation = () => new FollowRequest(Sender, Sender, SentAt);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Sender_withdraws_a_pending_request()
    {
        var request = CreatePendingRequest();

        var withdrawal = () => request.Withdraw(Sender);

        withdrawal.Should().NotThrow();
    }

    [Fact]
    public void Only_the_sender_can_withdraw_a_request()
    {
        var request = CreatePendingRequest();

        var withdrawal = () => request.Withdraw(Receiver);

        withdrawal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Request_that_is_no_longer_pending_cannot_be_withdrawn()
    {
        var request = CreateRejectedRequest();

        var withdrawal = () => request.Withdraw(Sender);

        withdrawal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Receiver_removes_a_follower()
    {
        var request = CreateAcceptedRequest();

        request.RemoveFollower(Receiver);

        request.Status.Should().Be(FollowRequestStatus.Removed);
    }

    [Fact]
    public void Only_the_receiver_can_remove_a_follower()
    {
        var request = CreateAcceptedRequest();

        var removal = () => request.RemoveFollower(Sender);

        removal.Should().Throw<NotFoundException>();
    }

    [Theory]
    [InlineData(FollowRequestStatus.Pending)]
    [InlineData(FollowRequestStatus.Unfollowed)]
    public void Follower_can_be_removed_only_from_an_accepted_request(FollowRequestStatus status)
    {
        var request = CreateRequestWithStatus(status);

        var removal = () => request.RemoveFollower(Receiver);

        removal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Sender_unfollows_the_followed_user()
    {
        var request = CreateAcceptedRequest();

        request.Unfollow(Sender);

        request.Status.Should().Be(FollowRequestStatus.Unfollowed);
    }

    [Fact]
    public void Only_the_sender_can_unfollow()
    {
        var request = CreateAcceptedRequest();

        var unfollowing = () => request.Unfollow(Receiver);

        unfollowing.Should().Throw<NotFoundException>();
    }

    [Theory]
    [InlineData(FollowRequestStatus.Pending)]
    [InlineData(FollowRequestStatus.Removed)]
    [InlineData(FollowRequestStatus.Unfollowed)]
    public void Sender_can_unfollow_only_while_following(FollowRequestStatus status)
    {
        var request = CreateRequestWithStatus(status);

        var unfollowing = () => request.Unfollow(Sender);

        unfollowing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Receiver_accepts_a_pending_request()
    {
        var request = CreatePendingRequest();

        request.Accept(Receiver);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public void Only_the_receiver_can_accept_a_request()
    {
        var request = CreatePendingRequest();

        var acceptance = () => request.Accept(Sender);

        acceptance.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Request_that_is_no_longer_pending_cannot_be_accepted()
    {
        var request = CreateRejectedRequest();

        var acceptance = () => request.Accept(Receiver);

        acceptance.Should().Throw<DomainException>();
    }

    [Fact]
    public void Receiver_rejects_a_pending_request()
    {
        var request = CreatePendingRequest();

        request.Reject(Receiver);

        request.Status.Should().Be(FollowRequestStatus.Rejected);
    }

    [Fact]
    public void Only_the_receiver_can_reject_a_request()
    {
        var request = CreatePendingRequest();

        var rejection = () => request.Reject(Sender);

        rejection.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Request_that_is_no_longer_pending_cannot_be_rejected()
    {
        var request = CreateAcceptedRequest();

        var rejection = () => request.Reject(Receiver);

        rejection.Should().Throw<DomainException>();
    }
}
