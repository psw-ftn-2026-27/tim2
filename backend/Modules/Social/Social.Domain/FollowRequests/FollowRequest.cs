using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.FollowRequests;

public sealed class FollowRequest : AggregateRoot
{
    public Guid SenderId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public FollowRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private FollowRequest() { }

    public FollowRequest(Guid senderId, Guid receiverId, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (senderId == receiverId)
        {
            throw new DomainException("A user cannot send a follow request to themselves.");
        }

        SenderId = senderId;
        ReceiverId = receiverId;
        Status = FollowRequestStatus.Pending;
        CreatedAt = createdAt;
    }

    public void Withdraw(Guid userId)
    {
        if (userId != SenderId)
        {
            throw new NotFoundException($"Follow request {Id} does not exist.");
        }
        EnsurePending();
    }

    public void AcceptAutomatically()
    {
        EnsurePending();

        Status = FollowRequestStatus.Accepted;
    }

    public void Accept(Guid userId)
    {
        if (userId != ReceiverId)
        {
            throw new NotFoundException($"Follow request {Id} does not exist.");
        }
        EnsurePending();

        Status = FollowRequestStatus.Accepted;
    }

    public void RemoveFollower(Guid userId)
    {
        if (userId != ReceiverId)
        {
            throw new NotFoundException($"Follow request {Id} does not exist.");
        }
        if (Status != FollowRequestStatus.Accepted)
        {
            throw new DomainException("Only an accepted follow request can be removed.");
        }

        Status = FollowRequestStatus.Removed;
    }

    public void Reject(Guid userId)
    {
        if (userId != ReceiverId)
        {
            throw new NotFoundException($"Follow request {Id} does not exist.");
        }
        EnsurePending();

        Status = FollowRequestStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != FollowRequestStatus.Pending)
        {
            throw new DomainException("The follow request is no longer pending.");
        }
    }
}
