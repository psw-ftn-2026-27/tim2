using Shared.Domain.Exceptions;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequestSending;

public sealed class FollowRequestSendingService
{
    private readonly IFollowRequestRepository _followRequestRepository;
    private readonly FollowRequestService _followRequestService;
    private readonly IUnitOfWork _unitOfWork;

    public FollowRequestSendingService(
        IFollowRequestRepository followRequestRepository,
        FollowRequestService followRequestService,
        IUnitOfWork unitOfWork)
    {
        _followRequestRepository = followRequestRepository;
        _followRequestService = followRequestService;
        _unitOfWork = unitOfWork;
    }

    public async Task SendAsync(Guid senderId, SendFollowRequestDto dto)
    {
        var requestsBetweenUsers = await _followRequestRepository.GetBetweenUsersAsync(senderId, dto.ReceiverId);

        var request = _followRequestService.Send(senderId, dto.ReceiverId, requestsBetweenUsers, DateTime.UtcNow);

        _followRequestRepository.Add(request);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawAsync(Guid requestId, Guid userId)
    {
        var request = await _followRequestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        request.Withdraw(userId);
        _followRequestRepository.Remove(request);
        await _unitOfWork.SaveChangesAsync();
    }
}
