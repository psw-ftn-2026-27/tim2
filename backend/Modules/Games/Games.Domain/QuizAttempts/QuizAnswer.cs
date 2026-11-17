namespace Games.Domain.QuizAttempts;

public sealed record QuizAnswer
{
    public int SelectedOptionIndex { get; private init; }
    public bool IsCorrect { get; private init; }
    public DateTime AnsweredAt { get; private init; }

    private QuizAnswer() { }

    public QuizAnswer(int selectedOptionIndex, bool isCorrect, DateTime answeredAt)
    {
        SelectedOptionIndex = selectedOptionIndex;
        IsCorrect = isCorrect;
        AnsweredAt = answeredAt;
    }
}
