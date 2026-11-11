using Shared.Domain;
using Social.Application.FollowRequests;

namespace Social.Application.Connections;

public sealed class ConnectionsQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;

    public ConnectionsQueries(IFollowRequestReadRepository followRequestReadRepository)
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
