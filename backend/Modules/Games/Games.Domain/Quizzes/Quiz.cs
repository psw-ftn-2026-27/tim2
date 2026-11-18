using Games.Domain.Locations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private const int MinimumNameLength = 3;
    private const int MaximumNameLength = 60;
    private const int MaximumInstructionsLength = 1000;
    private const int MinimumXp = 1;
    private const int MaximumXp = 1000;
    private const int MinimumQuestionCount = 1;
    private const int MaximumQuestionCount = 15;

    private readonly List<Question> _questions = [];

    public string Name { get; private set; }
    public string Instructions { get; private set; }
    public int Xp { get; private set; }
    public QuizStatus Status { get; private set; }
    public QuizLocation? Location { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    private Quiz()
    {
        Name = null!;
        Instructions = null!;
    }

    public Quiz(string name, string instructions, int xp, List<Question> questions, QuizLocation? location)
        : base(Guid.NewGuid())
    {
        Validate(name, instructions, xp, questions);

        Name = name.Trim();
        Instructions = instructions.Trim();
        Xp = xp;
        _questions.AddRange(questions);
        Location = location;
        Status = QuizStatus.Draft;
    }

    public void Update(string name, string instructions, int xp, List<Question> questions, QuizLocation? location)
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be changed.");
        }

        Validate(name, instructions, xp, questions);

        Name = name.Trim();
        Instructions = instructions.Trim();
        Xp = xp;
        _questions.Clear();
        _questions.AddRange(questions);
        Location = location;
    }

    public void EnsureCanBeDeleted()
    {
        EnsureStatusIsOneOf(QuizStatus.Draft, QuizStatus.Archived);
    }

    public void Publish()
    {
        EnsureStatusIsOneOf(QuizStatus.Draft);
        if (Location is null)
        {
            throw new DomainException("A quiz needs a location and a range before it can be published.");
        }

        Status = QuizStatus.Published;
    }

    public void Retire()
    {
        EnsureStatusIsOneOf(QuizStatus.Published);

        Status = QuizStatus.Retired;
    }

    public void Archive()
    {
        EnsureStatusIsOneOf(QuizStatus.Retired);

        Status = QuizStatus.Archived;
    }

    public void Repeat()
    {
        EnsureStatusIsOneOf(QuizStatus.Retired, QuizStatus.Archived);

        Status = QuizStatus.Draft;
    }

    public bool IsWithinRange(double latitude, double longitude)
    {
        return Location is not null
            && Location.Location.DistanceInMetersTo(new Location(latitude, longitude)) <= Location.RangeInMeters;
    }

    public bool IsCorrect(int questionIndex, int optionIndex)
    {
        if (questionIndex < 0 || questionIndex >= _questions.Count)
        {
            throw new DomainException("The quiz has no question with this number.");
        }
        var options = _questions[questionIndex].Options;
        if (optionIndex < 0 || optionIndex >= options.Count)
        {
            throw new DomainException("The question has no answer option with this number.");
        }

        return options[optionIndex].IsCorrect;
    }

    private void EnsureStatusIsOneOf(params QuizStatus[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(Status))
        {
            throw new DomainException($"This action is not allowed for a quiz in the {Status} state.");
        }
    }

    private static void Validate(string name, string instructions, int xp, List<Question> questions)
    {
        ValidateName(name);
        ValidateInstructions(instructions);
        ValidateXp(xp);
        ValidateQuestions(questions);
    }

    private static void ValidateName(string name)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length < MinimumNameLength || trimmedName.Length > MaximumNameLength)
        {
            throw new DomainException($"A quiz name must have from {MinimumNameLength} to {MaximumNameLength} characters.");
        }
    }

    private static void ValidateInstructions(string instructions)
    {
        var trimmedInstructions = instructions?.Trim() ?? string.Empty;
        if (trimmedInstructions.Length == 0)
        {
            throw new DomainException("A quiz requires instructions.");
        }
        if (trimmedInstructions.Length > MaximumInstructionsLength)
        {
            throw new DomainException($"Quiz instructions can have at most {MaximumInstructionsLength} characters.");
        }
    }

    private static void ValidateXp(int xp)
    {
        if (xp < MinimumXp || xp > MaximumXp)
        {
            throw new DomainException($"A quiz must carry from {MinimumXp} to {MaximumXp} XP.");
        }
    }

    private static void ValidateQuestions(List<Question> questions)
    {
        var questionCount = questions?.Count ?? 0;
        if (questionCount < MinimumQuestionCount || questionCount > MaximumQuestionCount)
        {
            throw new DomainException($"A quiz must have from {MinimumQuestionCount} to {MaximumQuestionCount} questions.");
        }
    }
}
