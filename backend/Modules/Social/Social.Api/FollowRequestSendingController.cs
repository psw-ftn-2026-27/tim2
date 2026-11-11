using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.FollowRequestSending;

namespace Social.Api;

[ApiController]
[Route("api/social/follow-requests")]
[Authorize]
public sealed class FollowRequestSendingController : ControllerBase
{
    private readonly FollowRequestSendingService _sendingService;
    private readonly FollowRequestSendingQueries _sendingQueries;

    public FollowRequestSendingController(
        FollowRequestSendingService sendingService,
        FollowRequestSendingQueries sendingQueries)
    {
        _sendingService = sendingService;
        _sendingQueries = sendingQueries;
    }

    [HttpPost]
    public async Task<ActionResult> Send(SendFollowRequestDto dto)
    {
        await _sendingService.SendAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Withdraw(Guid id)
    {
        await _sendingService.WithdrawAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("relationship/{userId:guid}")]
    public async Task<ActionResult<FollowRelationshipDto>> GetRelationship(Guid userId)
    {
        return await _sendingQueries.GetRelationshipAsync(User.GetUserId(), userId);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<List<SentFollowRequestDto>>> GetSent()
    {
        return await _sendingQueries.GetSentAsync(User.GetUserId());
    }
}
