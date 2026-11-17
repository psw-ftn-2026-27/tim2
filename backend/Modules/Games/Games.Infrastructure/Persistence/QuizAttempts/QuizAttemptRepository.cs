using Games.Application.QuizAttempts;
using Games.Domain.QuizAttempts;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal sealed class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizAttempt?> GetByQuizAndPlayerAsync(Guid quizId, Guid playerId)
    {
        return _dbContext.QuizAttempts.FirstOrDefaultAsync(attempt => attempt.QuizId == quizId && attempt.PlayerId == playerId);
    }

    public void Add(QuizAttempt attempt)
    {
        _dbContext.QuizAttempts.Add(attempt);
    }
}
