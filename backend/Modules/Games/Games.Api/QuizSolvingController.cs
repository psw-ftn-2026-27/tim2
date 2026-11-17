using Games.Application.QuizSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "explorer")]
public sealed class QuizSolvingController : ControllerBase
{
    private readonly QuizSolvingService _solvingService;
    private readonly QuizSolvingQueries _solvingQueries;

    public QuizSolvingController(QuizSolvingService solvingService, QuizSolvingQueries solvingQueries)
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

    [HttpPost("{id:guid}/answers")]
    public async Task<ActionResult> Answer(Guid id, AnswerQuizDto dto)
    {
        await _solvingService.AnswerAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<QuizProgressDto>> GetProgress(Guid id)
    {
        return await _solvingQueries.GetProgressAsync(id, User.GetUserId());
    }
}
