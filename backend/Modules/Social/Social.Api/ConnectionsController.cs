using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.Connections;

namespace Social.Api;

[ApiController]
[Route("api/social/connections")]
[Authorize]
public sealed class ConnectionsController : ControllerBase
{
    private readonly ConnectionsQueries _connectionsQueries;

    public ConnectionsController(ConnectionsQueries connectionsQueries)
    {
        _connectionsQueries = connectionsQueries;
    }

    [HttpGet("followers")]
    public async Task<ActionResult<PageResult<FollowerDto>>> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _connectionsQueries.GetFollowersAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("following")]
    public async Task<ActionResult<PageResult<FollowedUserDto>>> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _connectionsQueries.GetFollowingAsync(User.GetUserId(), page, pageSize);
    }
}
