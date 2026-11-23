using Games.Application.LocationChallengeManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeManagementController : ControllerBase
{
    private readonly LocationChallengeManagementService _managementService;
    private readonly LocationChallengeManagementQueries _managementQueries;

    public LocationChallengeManagementController(
        LocationChallengeManagementService managementService,
        LocationChallengeManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLocationChallengeDto dto)
    {
        return await _managementService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateLocationChallengeDto dto)
    {
        await _managementService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _managementService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationChallengeDto>> GetById(Guid id)
    {
        return await _managementQueries.GetByIdAsync(id);
    }
}
