using Games.Application.LocationChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengeSolvingController : ControllerBase
{
    private readonly LocationChallengeSolvingService _solvingService;
    private readonly LocationChallengeSolvingQueries _solvingQueries;

    public LocationChallengeSolvingController(LocationChallengeSolvingService solvingService, LocationChallengeSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id)
    {
        await _solvingService.ActivateAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/check")]
    public async Task<ActionResult> Check(Guid id)
    {
        await _solvingService.CheckAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<LocationChallengeProgressDto>> GetProgress(Guid id)
    {
        return await _solvingQueries.GetProgressAsync(id, User.GetUserId());
    }
}
