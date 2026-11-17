using Exploration.Application.KeyPointAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class KeyPointAuthoringController : ControllerBase
{
    private readonly KeyPointAuthoringService _authoringService;
    private readonly KeyPointAuthoringQueries _authoringQueries;

    public KeyPointAuthoringController(
        KeyPointAuthoringService authoringService,
        KeyPointAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet("{id:guid}/key-points")]
    public async Task<ActionResult<TourKeyPointsDto>> GetKeyPoints(Guid id)
    {
        return await _authoringQueries.GetKeyPointsAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/key-points")]
    public async Task<ActionResult> AddKeyPoint(Guid id, KeyPointDataDto dto)
    {
        await _authoringService.AddKeyPointAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> ChangeKeyPoint(Guid id, Guid keyPointId, KeyPointDataDto dto)
    {
        await _authoringService.ChangeKeyPointAsync(id, User.GetUserId(), keyPointId, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> RemoveKeyPoint(Guid id, Guid keyPointId)
    {
        await _authoringService.RemoveKeyPointAsync(id, User.GetUserId(), keyPointId);
        return NoContent();
    }

    [HttpPost("{id:guid}/key-points/{keyPointId:guid}/move")]
    public async Task<ActionResult> MoveKeyPoint(Guid id, Guid keyPointId, MoveKeyPointDto dto)
    {
        await _authoringService.MoveKeyPointAsync(id, User.GetUserId(), keyPointId, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/length")]
    public async Task<ActionResult> SetCalculatedLength(Guid id, TourLengthDto dto)
    {
        await _authoringService.SetCalculatedLengthAsync(id, User.GetUserId(), dto);
        return NoContent();
    }
}
