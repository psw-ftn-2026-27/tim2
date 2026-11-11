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
    private readonly FollowerManagingQueries _followerManagingQueries;

    public FollowerManagingController(FollowerManagingQueries followerManagingQueries)
    {
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
}
