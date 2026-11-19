using Exploration.Application.TourExecution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class TourExecutionController : ControllerBase
{
    private readonly TourExecutionService _executionService;
    private readonly TourExecutionQueries _executionQueries;

    public TourExecutionController(TourExecutionService executionService, TourExecutionQueries executionQueries)
    {
        _executionService = executionService;
        _executionQueries = executionQueries;
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult> Start(Guid id)
    {
        await _executionService.StartAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:guid}/abandon")]
    public async Task<ActionResult> Abandon(Guid id)
    {
        await _executionService.AbandonAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:guid}/check-arrival")]
    public async Task<ActionResult> CheckArrival(Guid id)
    {
        await _executionService.CheckArrivalAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<TourProgressDto?>> GetProgress(Guid id)
    {
        return await _executionQueries.GetTourProgressAsync(User.GetUserId(), id);
    }
}
