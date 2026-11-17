namespace Games.Application.QuizManagement;

public sealed record UpdateQuizDto(
    string Name,
    string Instructions,
    int Xp,
    List<QuestionDto> Questions,
    QuizLocationDto? Location);
