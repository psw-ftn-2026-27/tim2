using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record AnswerOption
{
    private const int MaximumTextLength = 100;

    public string Text { get; private init; }
    public bool IsCorrect { get; private init; }

    private AnswerOption()
    {
        Text = null!;
    }

    public AnswerOption(string text, bool isCorrect)
    {
        var trimmedText = text?.Trim() ?? string.Empty;
        if (trimmedText.Length == 0)
        {
            throw new DomainException("An answer option requires a text.");
        }
        if (trimmedText.Length > MaximumTextLength)
        {
            throw new DomainException($"An answer option text can have at most {MaximumTextLength} characters.");
        }

        Text = trimmedText;
        IsCorrect = isCorrect;
    }
}
