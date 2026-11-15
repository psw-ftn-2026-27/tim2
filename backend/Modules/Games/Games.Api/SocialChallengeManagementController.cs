using Games.Application.SocialChallengeManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeManagementController : ControllerBase
{
    private readonly SocialChallengeManagementService _managementService;

    public SocialChallengeManagementController(SocialChallengeManagementService managementService)
    {
        _managementService = managementService;
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
}
