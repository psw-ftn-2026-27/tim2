namespace Games.Application.QuizManagement;

public sealed record QuestionDto(string Text, List<AnswerOptionDto> Options);
