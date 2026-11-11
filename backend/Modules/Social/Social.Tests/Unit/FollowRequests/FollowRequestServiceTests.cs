using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.FollowRequests;
using Xunit;

namespace Social.Tests.Unit.FollowRequests;

public class FollowRequestServiceTests
{
    private static readonly Guid Sender = WellKnownUsers.Explorer;
    private static readonly Guid Receiver = WellKnownUsers.SecondExplorer;
    private static readonly DateTime EarlierAt = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime LaterAt = new(2026, 11, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime NowAt = new(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);

    private readonly FollowRequestService _service = new();

    private static FollowRequest CreateAcceptedRequest(Guid senderId, Guid receiverId, DateTime sentAt)
    {
        var request = new FollowRequest(senderId, receiverId, sentAt);
        request.AcceptAutomatically();
        return request;
    }

    private static FollowRequest CreateRejectedRequest(Guid senderId, Guid receiverId, DateTime sentAt)
    {
        var request = new FollowRequest(senderId, receiverId, sentAt);
        request.Reject(receiverId);
        return request;
    }

    private static FollowRequest CreateRemovedRequest(Guid senderId, Guid receiverId, DateTime sentAt)
    {
        var request = CreateAcceptedRequest(senderId, receiverId, sentAt);
        request.RemoveFollower(receiverId);
        return request;
    }

    [Fact]
    public void Second_request_while_the_first_is_pending_is_rejected()
    {
        var existing = new List<FollowRequest> { new(Sender, Receiver, EarlierAt) };

        var sending = () => _service.Send(Sender, Receiver, existing, NowAt);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_to_an_already_followed_user_is_rejected()
    {
        var existing = new List<FollowRequest> { CreateAcceptedRequest(Sender, Receiver, EarlierAt) };

        var sending = () => _service.Send(Sender, Receiver, existing, NowAt);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_to_a_user_who_follows_the_sender_is_accepted_automatically()
    {
        var existing = new List<FollowRequest> { CreateAcceptedRequest(Receiver, Sender, EarlierAt) };

        var request = _service.Send(Sender, Receiver, existing, NowAt);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public void Request_of_a_removed_sender_stays_pending_although_the_receiver_still_follows_them()
    {
        var existing = new List<FollowRequest>
        {
            CreateRemovedRequest(Sender, Receiver, EarlierAt),
            CreateAcceptedRequest(Receiver, Sender, EarlierAt)
        };

        var request = _service.Send(Sender, Receiver, existing, NowAt);

        request.Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public void New_request_can_be_sent_after_a_rejected_one()
    {
        var existing = new List<FollowRequest> { CreateRejectedRequest(Sender, Receiver, EarlierAt) };

        var request = _service.Send(Sender, Receiver, existing, NowAt);

        request.Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public void Request_to_a_follower_is_accepted_automatically_even_after_an_earlier_rejected_request()
    {
        var existing = new List<FollowRequest>
        {
            CreateRejectedRequest(Sender, Receiver, EarlierAt),
            CreateAcceptedRequest(Receiver, Sender, EarlierAt)
        };

        var request = _service.Send(Sender, Receiver, existing, NowAt);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public void After_removal_and_a_new_rejected_request_the_next_request_stays_pending()
    {
        var existing = new List<FollowRequest>
        {
            CreateRemovedRequest(Sender, Receiver, EarlierAt),
            CreateRejectedRequest(Sender, Receiver, LaterAt),
            CreateAcceptedRequest(Receiver, Sender, EarlierAt)
        };

        var request = _service.Send(Sender, Receiver, existing, NowAt);

        request.Status.Should().Be(FollowRequestStatus.Pending);
    }
}
