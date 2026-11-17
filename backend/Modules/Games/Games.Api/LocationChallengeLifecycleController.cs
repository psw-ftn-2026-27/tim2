using Games.Application.LocationChallengeLifecycle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeLifecycleController : ControllerBase
{
    private readonly LocationChallengeLifecycleService _lifecycleService;

    public LocationChallengeLifecycleController(LocationChallengeLifecycleService lifecycleService)
    {
        _lifecycleService = lifecycleService;
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _lifecycleService.PublishAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/retire")]
    public async Task<ActionResult> Retire(Guid id)
    {
        await _lifecycleService.RetireAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await _lifecycleService.ArchiveAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/repeat")]
    public async Task<ActionResult> Repeat(Guid id)
    {
        await _lifecycleService.RepeatAsync(id);
        return NoContent();
    }
}
