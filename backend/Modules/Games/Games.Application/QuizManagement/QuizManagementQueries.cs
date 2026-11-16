using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizManagement;

public sealed class QuizManagementQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizManagementQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public async Task<QuizDto> GetByIdAsync(Guid id)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(id);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {id} does not exist.");
        }

        return quiz;
    }
}
