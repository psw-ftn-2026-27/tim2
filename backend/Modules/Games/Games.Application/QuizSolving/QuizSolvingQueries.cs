using Games.Application.QuizAttempts;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingQueries
{
    private readonly IQuizAttemptReadRepository _attemptReadRepository;

    public QuizSolvingQueries(IQuizAttemptReadRepository attemptReadRepository)
    {
        _attemptReadRepository = attemptReadRepository;
    }

    public async Task<QuizProgressDto> GetProgressAsync(Guid quizId, Guid playerId)
    {
        var progress = await _attemptReadRepository.GetProgressAsync(quizId, playerId);
        if (progress is null)
        {
            throw new NotFoundException($"You have not activated quiz {quizId}.");
        }

        return progress;
    }
}
