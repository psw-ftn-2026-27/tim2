using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.FollowerManaging;

namespace Social.Api;

[ApiController]
[Route("api/social/follow-requests")]
[Authorize]
public sealed class FollowerManagingController : ControllerBase
{
    private readonly FollowerManagingService _followerManagingService;
    private readonly FollowerManagingQueries _followerManagingQueries;

    public FollowerManagingController(
        FollowerManagingService followerManagingService,
        FollowerManagingQueries followerManagingQueries)
    {
        _followerManagingService = followerManagingService;
        _followerManagingQueries = followerManagingQueries;
    }

    [HttpGet("followers")]
    public async Task<ActionResult<PageResult<FollowerDto>>> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _followerManagingQueries.GetFollowersAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("following")]
    public async Task<ActionResult<PageResult<FollowedUserDto>>> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _followerManagingQueries.GetFollowingAsync(User.GetUserId(), page, pageSize);
    }

    [HttpPost("followers/{id:guid}/remove")]
    public async Task<ActionResult> RemoveFollower(Guid id)
    {
        await _followerManagingService.RemoveFollowerAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("following/{id:guid}/unfollow")]
    public async Task<ActionResult> Unfollow(Guid id)
    {
        await _followerManagingService.UnfollowAsync(id, User.GetUserId());
        return NoContent();
    }
}
