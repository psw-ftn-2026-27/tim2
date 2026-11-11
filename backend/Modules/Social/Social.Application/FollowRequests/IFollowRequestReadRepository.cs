using Shared.Domain;
using Social.Application.FollowerManaging;
using Social.Application.FollowRequestResponding;
using Social.Application.FollowRequestSending;

namespace Social.Application.FollowRequests;

public interface IFollowRequestReadRepository
{
    Task<FollowRelationshipDto> GetRelationshipAsync(Guid userId, Guid otherUserId);
    Task<List<SentFollowRequestDto>> GetSentAsync(Guid senderId);
    Task<List<ReceivedFollowRequestDto>> GetReceivedAsync(Guid receiverId);
    Task<PageResult<FollowerDto>> GetFollowersAsync(Guid userId, int page, int pageSize);
    Task<PageResult<FollowedUserDto>> GetFollowingAsync(Guid userId, int page, int pageSize);
}
