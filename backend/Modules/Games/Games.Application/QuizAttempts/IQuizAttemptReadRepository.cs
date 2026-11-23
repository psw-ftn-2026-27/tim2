using Games.Application.QuizSolving;

namespace Games.Application.QuizAttempts;

public interface IQuizAttemptReadRepository
{
    Task<QuizProgressDto?> GetProgressAsync(Guid quizId, Guid playerId);
}
