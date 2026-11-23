using Shared.Domain.Exceptions;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Application.FollowerManaging;

public sealed class FollowerManagingService
{
    private readonly IFollowRequestRepository _followRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowerManagingService(
        IFollowRequestRepository followRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _followRequestRepository = followRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RemoveFollowerAsync(Guid requestId, Guid userId)
    {
        var request = await GetRequestAsync(requestId);

        request.RemoveFollower(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid requestId, Guid userId)
    {
        var request = await GetRequestAsync(requestId);

        request.Unfollow(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<FollowRequest> GetRequestAsync(Guid requestId)
    {
        var request = await _followRequestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        return request;
    }
}
