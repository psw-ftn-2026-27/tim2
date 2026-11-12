using Exploration.Application.TourPreferenceSetting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-preferences")]
[Authorize(Roles = "explorer")]
public sealed class TourPreferenceSettingController : ControllerBase
{
    private readonly TourPreferenceSettingService _settingService;
    private readonly TourPreferenceSettingQueries _settingQueries;

    public TourPreferenceSettingController(
        TourPreferenceSettingService settingService,
        TourPreferenceSettingQueries settingQueries)
    {
        _settingService = settingService;
        _settingQueries = settingQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<TourPreferenceDto?>> GetMine()
    {
        return await _settingQueries.GetMineAsync(User.GetUserId());
    }

    [HttpPut("mine")]
    public async Task<ActionResult> Save(TourPreferenceDto dto)
    {
        await _settingService.SaveAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("mine")]
    public async Task<ActionResult> Delete()
    {
        await _settingService.DeleteAsync(User.GetUserId());
        return NoContent();
    }
}
