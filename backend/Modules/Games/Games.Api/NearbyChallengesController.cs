using Games.Application.NearbyChallenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/nearby-challenges")]
[Authorize(Roles = "explorer")]
public sealed class NearbyChallengesController : ControllerBase
{
    private readonly NearbyChallengesQueries _nearbyQueries;

    public NearbyChallengesController(NearbyChallengesQueries nearbyQueries)
    {
        _nearbyQueries = nearbyQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<NearbyChallengeDto>>> GetAll()
    {
        return await _nearbyQueries.GetAllAsync(User.GetUserId());
    }
}
