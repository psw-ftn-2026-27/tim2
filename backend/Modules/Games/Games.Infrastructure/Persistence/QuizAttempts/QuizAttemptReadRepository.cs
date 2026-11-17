using Games.Application.QuizAttempts;
using Games.Application.QuizSolving;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal sealed class QuizAttemptReadRepository : IQuizAttemptReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<QuizProgressDto?> GetProgressAsync(Guid quizId, Guid playerId)
    {
        var attempt = await _dbContext.QuizAttempts
            .AsNoTracking()
            .Where(attempt => attempt.QuizId == quizId && attempt.PlayerId == playerId)
            .Select(attempt => new
            {
                attempt.QuestionCount,
                attempt.Answers,
                attempt.SolvedAt,
                attempt.EarnedXp
            })
            .FirstOrDefaultAsync();
        if (attempt is null)
        {
            return null;
        }

        var correctCount = attempt.Answers.Count(answer => answer.IsCorrect);
        var lastAnswerCorrect = attempt.Answers.Count == 0 ? (bool?)null : attempt.Answers[^1].IsCorrect;

        if (attempt.SolvedAt is not null)
        {
            return new QuizProgressDto(
                quizId,
                attempt.QuestionCount,
                correctCount,
                lastAnswerCorrect,
                null,
                correctCount * 100 / attempt.QuestionCount,
                attempt.EarnedXp,
                attempt.SolvedAt);
        }

        var nextQuestionIndex = attempt.Answers.Count;
        var questions = await _dbContext.Quizzes
            .AsNoTracking()
            .Where(quiz => quiz.Id == quizId)
            .Select(quiz => quiz.Questions)
            .FirstOrDefaultAsync();

        // The question is missing only when the quiz lost questions after activation; that attempt stays locked.
        var currentQuestion = questions is null || nextQuestionIndex >= questions.Count
            ? null
            : new CurrentQuestionDto(
                nextQuestionIndex,
                questions[nextQuestionIndex].Text,
                questions[nextQuestionIndex].Options.Select(option => option.Text).ToList());

        return new QuizProgressDto(
            quizId,
            attempt.QuestionCount,
            correctCount,
            lastAnswerCorrect,
            currentQuestion,
            null,
            null,
            null);
    }
}
