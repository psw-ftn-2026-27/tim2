using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Positions.Application.PositionSimulating;
using Positions.Application.UserPositions;
using Shared.Api;

namespace Positions.Api;

[ApiController]
[Route("api/positions")]
[Authorize(Roles = "explorer")]
public sealed class PositionSimulatingController : ControllerBase
{
    private readonly PositionSimulatingService _simulatingService;
    private readonly PositionSimulatingQueries _simulatingQueries;

    public PositionSimulatingController(
        PositionSimulatingService simulatingService,
        PositionSimulatingQueries simulatingQueries)
    {
        _simulatingService = simulatingService;
        _simulatingQueries = simulatingQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<UserPositionDto?>> GetMine()
    {
        return await _simulatingQueries.GetMyPositionAsync(User.GetUserId());
    }

    [HttpPut("mine")]
    public async Task<ActionResult> SetMine(UserPositionDto dto)
    {
        await _simulatingService.SetMyPositionAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
