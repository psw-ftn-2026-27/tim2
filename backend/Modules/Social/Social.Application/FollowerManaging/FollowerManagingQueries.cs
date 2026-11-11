using Shared.Domain;
using Social.Application.FollowRequests;

namespace Social.Application.FollowerManaging;

public sealed class FollowerManagingQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;

    public FollowerManagingQueries(IFollowRequestReadRepository followRequestReadRepository)
    {
        _followRequestReadRepository = followRequestReadRepository;
    }

    public Task<PageResult<FollowerDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _followRequestReadRepository.GetFollowersAsync(userId, page, pageSize);
    }

    public Task<PageResult<FollowedUserDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _followRequestReadRepository.GetFollowingAsync(userId, page, pageSize);
    }
}
