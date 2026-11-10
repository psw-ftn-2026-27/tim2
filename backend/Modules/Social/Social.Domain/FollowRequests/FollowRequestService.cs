using Shared.Domain.Exceptions;

namespace Social.Domain.FollowRequests;

public sealed class FollowRequestService
{
    public FollowRequest Send(Guid senderId, Guid receiverId, IReadOnlyList<FollowRequest> requestsBetweenUsers, DateTime sentAt)
    {
        var request = new FollowRequest(senderId, receiverId, sentAt);

        var sentRequests = requestsBetweenUsers
            .Where(existing => existing.SenderId == senderId && existing.ReceiverId == receiverId)
            .ToList();
        if (sentRequests.Any(existing => existing.Status == FollowRequestStatus.Pending))
        {
            throw new DomainException("A follow request to this user is already pending.");
        }
        if (sentRequests.Any(existing => existing.Status == FollowRequestStatus.Accepted))
        {
            throw new DomainException("You already follow this user.");
        }

        var receiverFollowsSender = requestsBetweenUsers.Any(existing =>
            existing.SenderId == receiverId &&
            existing.ReceiverId == senderId &&
            existing.Status == FollowRequestStatus.Accepted);
        var lastEndedConnection = sentRequests
            .Where(existing => existing.Status is FollowRequestStatus.Removed or FollowRequestStatus.Unfollowed)
            .MaxBy(existing => existing.CreatedAt);
        var senderWasRemoved = lastEndedConnection?.Status == FollowRequestStatus.Removed;

        if (receiverFollowsSender && !senderWasRemoved)
        {
            request.AcceptAutomatically();
        }

        return request;
    }
}
