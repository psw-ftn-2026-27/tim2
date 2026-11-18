using Games.Application.SocialChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "explorer")]
public sealed class SocialChallengeSolvingController : ControllerBase
{
    private readonly SocialChallengeSolvingService _solvingService;
    private readonly SocialChallengeSolvingQueries _solvingQueries;

    public SocialChallengeSolvingController(SocialChallengeSolvingService solvingService, SocialChallengeSolvingQueries solvingQueries)
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

    [HttpPost("{id:guid}/checks")]
    public async Task<ActionResult> Check(Guid id)
    {
        await _solvingService.CheckAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<SocialChallengeProgressDto>> GetProgress(Guid id)
    {
        return await _solvingQueries.GetProgressAsync(id, User.GetUserId());
    }
}
