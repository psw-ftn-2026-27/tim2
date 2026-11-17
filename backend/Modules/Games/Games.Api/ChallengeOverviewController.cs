using Games.Application.ChallengeOverview;
using Games.Application.Challenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "administrator")]
public sealed class ChallengeOverviewController : ControllerBase
{
    private readonly ChallengeOverviewQueries _overviewQueries;

    public ChallengeOverviewController(ChallengeOverviewQueries overviewQueries)
    {
        _overviewQueries = overviewQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<ChallengeDto>>> GetAll(
        [FromQuery] ChallengeType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _overviewQueries.GetAllAsync(type, page, pageSize);
    }
}
