using Games.Application.QuizManagement;
using Games.Application.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.Quizzes
            .Where(quiz => quiz.Id == id)
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Xp,
                quiz.Status,
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Text,
                        question.Options
                            .Select(option => new AnswerOptionDto(option.Text, option.IsCorrect))
                            .ToList()))
                    .ToList(),
                quiz.Location == null
                    ? null
                    : new QuizLocationDto(quiz.Location.Location.Latitude, quiz.Location.Location.Longitude, quiz.Location.RangeInMeters)))
            .FirstOrDefaultAsync();
    }
}
