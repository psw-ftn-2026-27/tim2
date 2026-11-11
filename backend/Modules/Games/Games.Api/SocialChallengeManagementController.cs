using Games.Application.SocialChallengeManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeManagementController : ControllerBase
{
    private readonly SocialChallengeManagementService _managementService;
    private readonly SocialChallengeManagementQueries _managementQueries;

    public SocialChallengeManagementController(
        SocialChallengeManagementService managementService,
        SocialChallengeManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateSocialChallengeDto dto)
    {
        return await _managementService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateSocialChallengeDto dto)
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

    [HttpGet]
    public async Task<ActionResult<PageResult<SocialChallengeDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetAllAsync(page, pageSize);
    }
}
