namespace Games.Application.QuizManagement;

public sealed record CreateQuizDto(
    string Name,
    string Instructions,
    int Xp,
    List<QuestionDto> Questions);
