using Social.Application.FollowRequests;

namespace Social.Application.FollowRequestResponding;

public sealed class FollowRequestRespondingQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;

    public FollowRequestRespondingQueries(IFollowRequestReadRepository followRequestReadRepository)
    {
        _followRequestReadRepository = followRequestReadRepository;
    }

    public Task<List<ReceivedFollowRequestDto>> GetReceivedAsync(Guid receiverId)
    {
        return _followRequestReadRepository.GetReceivedAsync(receiverId);
    }
}
