using Games.Domain.Quizzes;

namespace Games.Application.QuizManagement;

public sealed record QuizDto(
    Guid Id,
    string Name,
    string Instructions,
    int Xp,
    QuizStatus Status,
    List<QuestionDto> Questions,
    QuizLocationDto? Location);
