using Shared.Domain.Exceptions;

namespace Social.Domain.FollowRequests;

public sealed class FollowRequestService
{
    public FollowRequest Send(Guid senderId, Guid receiverId, IReadOnlyList<FollowRequest> requestsBetweenUsers, DateTime sentAt)
    {
        var sentRequests = requestsBetweenUsers
            .Where(existing => existing.SenderId == senderId && existing.ReceiverId == receiverId)
            .ToList();
        EnsureCanSend(sentRequests);

        var request = new FollowRequest(senderId, receiverId, sentAt);
        if (ReceiverFollowsSender(senderId, receiverId, requestsBetweenUsers) && !SenderWasRemoved(sentRequests))
        {
            request.AcceptAutomatically();
        }

        return request;
    }

    private static void EnsureCanSend(List<FollowRequest> sentRequests)
    {
        if (sentRequests.Any(existing => existing.Status == FollowRequestStatus.Pending))
        {
            throw new DomainException("A follow request to this user is already pending.");
        }
        if (sentRequests.Any(existing => existing.Status == FollowRequestStatus.Accepted))
        {
            throw new DomainException("You already follow this user.");
        }
    }

    private static bool ReceiverFollowsSender(Guid senderId, Guid receiverId, IReadOnlyList<FollowRequest> requestsBetweenUsers)
    {
        return requestsBetweenUsers.Any(existing =>
            existing.SenderId == receiverId &&
            existing.ReceiverId == senderId &&
            existing.Status == FollowRequestStatus.Accepted);
    }

    private static bool SenderWasRemoved(List<FollowRequest> sentRequests)
    {
        var lastEndedConnection = sentRequests
            .Where(existing => existing.Status is FollowRequestStatus.Removed or FollowRequestStatus.Unfollowed)
            .MaxBy(existing => existing.CreatedAt);

        return lastEndedConnection?.Status == FollowRequestStatus.Removed;
    }
}
