using Games.Domain.QuizAttempts;

namespace Games.Application.QuizAttempts;

public interface IQuizAttemptRepository
{
    Task<QuizAttempt?> GetByQuizAndPlayerAsync(Guid quizId, Guid playerId);
    void Add(QuizAttempt attempt);
}
