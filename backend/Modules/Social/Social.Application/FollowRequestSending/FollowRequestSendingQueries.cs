using Social.Application.FollowRequests;

namespace Social.Application.FollowRequestSending;

public sealed class FollowRequestSendingQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;

    public FollowRequestSendingQueries(IFollowRequestReadRepository followRequestReadRepository)
    {
        _followRequestReadRepository = followRequestReadRepository;
    }

    public Task<FollowRelationshipDto> GetRelationshipAsync(Guid userId, Guid otherUserId)
    {
        return _followRequestReadRepository.GetRelationshipAsync(userId, otherUserId);
    }

    public Task<List<SentFollowRequestDto>> GetSentAsync(Guid senderId)
    {
        return _followRequestReadRepository.GetSentAsync(senderId);
    }
}
