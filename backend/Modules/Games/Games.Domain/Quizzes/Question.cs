using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record Question
{
    private const int MaximumTextLength = 200;
    private const int MinimumOptionCount = 2;
    private const int MaximumOptionCount = 6;

    private readonly List<AnswerOption> _options = [];

    public string Text { get; private init; }
    public IReadOnlyList<AnswerOption> Options => _options;

    private Question()
    {
        Text = null!;
    }

    public Question(string text, List<AnswerOption> options)
    {
        var trimmedText = text?.Trim() ?? string.Empty;
        options ??= [];
        ValidateText(trimmedText);
        ValidateOptions(options);

        Text = trimmedText;
        _options.AddRange(options);
    }

    private static void ValidateText(string trimmedText)
    {
        if (trimmedText.Length == 0)
        {
            throw new DomainException("A question requires a text.");
        }
        if (trimmedText.Length > MaximumTextLength)
        {
            throw new DomainException($"A question text can have at most {MaximumTextLength} characters.");
        }
    }

    private static void ValidateOptions(List<AnswerOption> options)
    {
        if (options.Count < MinimumOptionCount || options.Count > MaximumOptionCount)
        {
            throw new DomainException($"A question must have from {MinimumOptionCount} to {MaximumOptionCount} answer options.");
        }
        if (options.Count(option => option.IsCorrect) != 1)
        {
            throw new DomainException("A question must have exactly one correct answer option.");
        }
        // AnswerOption already trims its text, so only letter case is left to ignore.
        if (options.Select(option => option.Text.ToLowerInvariant()).Distinct().Count() != options.Count)
        {
            throw new DomainException("Two answer options of the same question cannot be the same.");
        }
    }
}
