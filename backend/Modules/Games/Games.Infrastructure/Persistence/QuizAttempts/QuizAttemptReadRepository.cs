using Games.Application.QuizAttempts;
using Games.Application.QuizSolving;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
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
        var attempt = await FindAttemptAsync(quizId, playerId);
        if (attempt is null)
        {
            return null;
        }

        var questions = await FindQuestionsAsync(quizId);
        return ToProgress(quizId, attempt, questions);
    }

    private async Task<AttemptRow?> FindAttemptAsync(Guid quizId, Guid playerId)
    {
        return await _dbContext.QuizAttempts
            .AsNoTracking()
            .Where(attempt => attempt.QuizId == quizId && attempt.PlayerId == playerId)
            .Select(attempt => new AttemptRow
            {
                QuestionCount = attempt.QuestionCount,
                Answers = attempt.Answers,
                SolvedAt = attempt.SolvedAt,
                EarnedXp = attempt.EarnedXp
            })
            .FirstOrDefaultAsync();
    }

    private async Task<IReadOnlyList<Question>?> FindQuestionsAsync(Guid quizId)
    {
        return await _dbContext.Quizzes
            .AsNoTracking()
            .Where(quiz => quiz.Id == quizId)
            .Select(quiz => quiz.Questions)
            .FirstOrDefaultAsync();
    }

    private static QuizProgressDto ToProgress(Guid quizId, AttemptRow attempt, IReadOnlyList<Question>? questions)
    {
        return attempt.SolvedAt is null
            ? ToProgressInProgress(quizId, attempt, questions)
            : ToSolvedProgress(quizId, attempt);
    }

    private static QuizProgressDto ToSolvedProgress(Guid quizId, AttemptRow attempt)
    {
        var correctCount = CorrectCount(attempt);
        return new QuizProgressDto(
            quizId,
            attempt.QuestionCount,
            correctCount,
            LastAnswerCorrect(attempt),
            null,
            QuizAttempt.PercentageOf(correctCount, attempt.QuestionCount),
            attempt.EarnedXp,
            attempt.SolvedAt);
    }

    private static QuizProgressDto ToProgressInProgress(Guid quizId, AttemptRow attempt, IReadOnlyList<Question>? questions)
    {
        return new QuizProgressDto(
            quizId,
            attempt.QuestionCount,
            CorrectCount(attempt),
            LastAnswerCorrect(attempt),
            ToCurrentQuestion(attempt.Answers.Count, questions),
            null,
            null,
            null);
    }

    // The question is missing only when the quiz lost questions after activation; that attempt stays locked.
    private static CurrentQuestionDto? ToCurrentQuestion(int nextQuestionIndex, IReadOnlyList<Question>? questions)
    {
        if (questions is null || nextQuestionIndex >= questions.Count)
        {
            return null;
        }

        var question = questions[nextQuestionIndex];
        return new CurrentQuestionDto(nextQuestionIndex, question.Text, question.Options.Select(option => option.Text).ToList());
    }

    private static int CorrectCount(AttemptRow attempt)
    {
        return attempt.Answers.Count(answer => answer.IsCorrect);
    }

    private static bool? LastAnswerCorrect(AttemptRow attempt)
    {
        return attempt.Answers.Count == 0 ? null : attempt.Answers[^1].IsCorrect;
    }

    private sealed class AttemptRow
    {
        public int QuestionCount { get; init; }
        public IReadOnlyList<QuizAnswer> Answers { get; init; } = null!;
        public DateTime? SolvedAt { get; init; }
        public int? EarnedXp { get; init; }
    }
}
