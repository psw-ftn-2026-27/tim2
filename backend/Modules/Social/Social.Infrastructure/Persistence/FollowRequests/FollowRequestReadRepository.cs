using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.FollowerManaging;
using Social.Application.FollowRequestResponding;
using Social.Application.FollowRequestSending;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Infrastructure.Persistence.FollowRequests;

internal sealed class FollowRequestReadRepository : IFollowRequestReadRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRequestReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FollowRelationshipDto> GetRelationshipAsync(Guid userId, Guid otherUserId)
    {
        var sentRequests = _dbContext.FollowRequests
            .Where(request => request.SenderId == userId && request.ReceiverId == otherUserId);

        var isFollowing = await sentRequests.AnyAsync(request => request.Status == FollowRequestStatus.Accepted);
        var pendingRequestId = await sentRequests
            .Where(request => request.Status == FollowRequestStatus.Pending)
            .Select(request => (Guid?)request.Id)
            .FirstOrDefaultAsync();

        return ToRelationship(isFollowing, pendingRequestId);
    }

    public Task<List<SentFollowRequestDto>> GetSentAsync(Guid senderId)
    {
        return _dbContext.FollowRequests
            .Where(request => request.SenderId == senderId)
            .Where(request => !_dbContext.FollowRequests.Any(newer =>
                newer.SenderId == request.SenderId &&
                newer.ReceiverId == request.ReceiverId &&
                newer.CreatedAt > request.CreatedAt))
            .Where(request =>
                request.Status == FollowRequestStatus.Pending ||
                request.Status == FollowRequestStatus.Rejected)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new SentFollowRequestDto(
                request.Id, request.ReceiverId, request.Status, request.CreatedAt))
            .ToListAsync();
    }

    public Task<List<ReceivedFollowRequestDto>> GetReceivedAsync(Guid receiverId)
    {
        return _dbContext.FollowRequests
            .Where(request => request.ReceiverId == receiverId && request.Status == FollowRequestStatus.Pending)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new ReceivedFollowRequestDto(request.Id, request.SenderId, request.CreatedAt))
            .ToListAsync();
    }

    public async Task<PageResult<FollowerDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        var followers = _dbContext.FollowRequests
            .Where(request => request.ReceiverId == userId && request.Status == FollowRequestStatus.Accepted)
            .OrderByDescending(request => request.CreatedAt)
            .ThenBy(request => request.Id);

        var totalCount = await followers.CountAsync();
        var rows = await followers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new FollowerRow(
                request.SenderId,
                _dbContext.FollowRequests.Any(sent =>
                    sent.SenderId == userId &&
                    sent.ReceiverId == request.SenderId &&
                    sent.Status == FollowRequestStatus.Accepted),
                _dbContext.FollowRequests
                    .Where(sent =>
                        sent.SenderId == userId &&
                        sent.ReceiverId == request.SenderId &&
                        sent.Status == FollowRequestStatus.Pending)
                    .Select(sent => (Guid?)sent.Id)
                    .FirstOrDefault()))
            .ToListAsync();

        var items = rows.Select(ToFollowerDto).ToList();

        return new PageResult<FollowerDto>(items, totalCount);
    }

    public async Task<PageResult<FollowedUserDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        var following = _dbContext.FollowRequests
            .Where(request => request.SenderId == userId && request.Status == FollowRequestStatus.Accepted)
            .OrderByDescending(request => request.CreatedAt)
            .ThenBy(request => request.Id);

        var totalCount = await following.CountAsync();
        var items = await following
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new FollowedUserDto(request.ReceiverId))
            .ToListAsync();

        return new PageResult<FollowedUserDto>(items, totalCount);
    }

    private static FollowerDto ToFollowerDto(FollowerRow row)
    {
        var relationship = ToRelationship(row.IsFollowedBack, row.PendingRequestId);
        return new FollowerDto(row.FollowerId, relationship.State, relationship.PendingRequestId);
    }

    private static FollowRelationshipDto ToRelationship(bool isFollowing, Guid? pendingRequestId)
    {
        if (isFollowing)
        {
            return new FollowRelationshipDto(FollowRelationshipState.Following, null);
        }

        return pendingRequestId is null
            ? new FollowRelationshipDto(FollowRelationshipState.Follow, null)
            : new FollowRelationshipDto(FollowRelationshipState.Pending, pendingRequestId);
    }

    private sealed record FollowerRow(Guid FollowerId, bool IsFollowedBack, Guid? PendingRequestId);
}
