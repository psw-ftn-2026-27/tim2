using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.FollowRequestResponding;

namespace Social.Api;

[ApiController]
[Route("api/social/follow-requests")]
[Authorize]
public sealed class FollowRequestRespondingController : ControllerBase
{
    private readonly FollowRequestRespondingService _respondingService;
    private readonly FollowRequestRespondingQueries _respondingQueries;

    public FollowRequestRespondingController(
        FollowRequestRespondingService respondingService,
        FollowRequestRespondingQueries respondingQueries)
    {
        _respondingService = respondingService;
        _respondingQueries = respondingQueries;
    }

    [HttpGet("received")]
    public async Task<ActionResult<List<ReceivedFollowRequestDto>>> GetReceived()
    {
        return await _respondingQueries.GetReceivedAsync(User.GetUserId());
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult> Accept(Guid id)
    {
        await _respondingService.AcceptAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult> Reject(Guid id)
    {
        await _respondingService.RejectAsync(id, User.GetUserId());
        return NoContent();
    }
}
