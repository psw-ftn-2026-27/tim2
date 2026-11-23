using Games.Application.QuizManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizManagementController : ControllerBase
{
    private readonly QuizManagementService _managementService;
    private readonly QuizManagementQueries _managementQueries;

    public QuizManagementController(
        QuizManagementService managementService,
        QuizManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateQuizDto dto)
    {
        return await _managementService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateQuizDto dto)
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
    public async Task<ActionResult<QuizDto>> GetById(Guid id)
    {
        return await _managementQueries.GetByIdAsync(id);
    }
}
