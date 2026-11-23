using Exploration.Application.TourLocationSearchRecording;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-location-searches")]
[Authorize(Roles = "explorer")]
public sealed class TourLocationSearchRecordingController : ControllerBase
{
    private readonly TourLocationSearchRecordingService _recordingService;

    public TourLocationSearchRecordingController(TourLocationSearchRecordingService recordingService)
    {
        _recordingService = recordingService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> SaveLocationSearch(LocationSearchRecordDto dto)
    {
        return await _recordingService.SaveLocationSearchAsync(dto);
    }
}
