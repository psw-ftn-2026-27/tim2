using Games.Application.QuizManagement;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<QuizDto?> GetByIdAsync(Guid id);
}
