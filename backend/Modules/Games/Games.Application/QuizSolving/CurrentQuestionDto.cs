namespace Games.Application.QuizSolving;

// The answer options are listed by text only, so the player cannot see which one is correct.
public sealed record CurrentQuestionDto(int Index, string Text, List<string> Options);
