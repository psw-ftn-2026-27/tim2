using Microsoft.EntityFrameworkCore;
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

        if (await sentRequests.AnyAsync(request => request.Status == FollowRequestStatus.Accepted))
        {
            return new FollowRelationshipDto(FollowRelationshipState.Following, null);
        }

        var pendingRequestId = await sentRequests
            .Where(request => request.Status == FollowRequestStatus.Pending)
            .Select(request => (Guid?)request.Id)
            .FirstOrDefaultAsync();

        return pendingRequestId is null
            ? new FollowRelationshipDto(FollowRelationshipState.Follow, null)
            : new FollowRelationshipDto(FollowRelationshipState.Pending, pendingRequestId);
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
}
