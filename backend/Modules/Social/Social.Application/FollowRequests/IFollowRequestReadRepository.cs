using Social.Application.FollowRequestSending;

namespace Social.Application.FollowRequests;

public interface IFollowRequestReadRepository
{
    Task<FollowRelationshipDto> GetRelationshipAsync(Guid userId, Guid otherUserId);
    Task<List<SentFollowRequestDto>> GetSentAsync(Guid senderId);
}
