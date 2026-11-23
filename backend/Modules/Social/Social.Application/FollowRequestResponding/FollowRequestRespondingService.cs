using Shared.Domain.Exceptions;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequestResponding;

public sealed class FollowRequestRespondingService
{
    private readonly IFollowRequestRepository _followRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowRequestRespondingService(
        IFollowRequestRepository followRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _followRequestRepository = followRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AcceptAsync(Guid requestId, Guid userId)
    {
        var request = await GetRequestAsync(requestId);

        request.Accept(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid requestId, Guid userId)
    {
        var request = await GetRequestAsync(requestId);

        request.Reject(userId);
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
