using Exploration.Application.TourPublishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class TourPublishingController : ControllerBase
{
    private readonly TourPublishingService _publishingService;

    public TourPublishingController(TourPublishingService publishingService)
    {
        _publishingService = publishingService;
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _publishingService.PublishAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await _publishingService.ArchiveAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult> Reactivate(Guid id)
    {
        await _publishingService.ReactivateAsync(id, User.GetUserId());
        return NoContent();
    }
}
