namespace Games.Application.QuizSolving;

// While the attempt is in progress, CurrentQuestion holds the first unanswered question and the result fields are null.
// Once it is solved, CurrentQuestion is null and the result fields are filled.
public sealed record QuizProgressDto(
    Guid QuizId,
    int QuestionCount,
    int CorrectCount,
    bool? LastAnswerCorrect,
    CurrentQuestionDto? CurrentQuestion,
    int? Percentage,
    int? EarnedXp,
    DateTime? SolvedAt);
