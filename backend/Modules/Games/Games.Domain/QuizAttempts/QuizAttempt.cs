using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

public sealed class QuizAttempt : AggregateRoot
{
    private readonly List<QuizAnswer> _answers = [];

    public Guid QuizId { get; private set; }
    public Guid PlayerId { get; private set; }
    public DateTime ActivatedAt { get; private set; }
    public int QuestionCount { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? EarnedXp { get; private set; }
    public IReadOnlyList<QuizAnswer> Answers => _answers;

    private QuizAttempt() { }

    public QuizAttempt(Guid quizId, Guid playerId, int questionCount, DateTime activatedAt)
        : base(Guid.NewGuid())
    {
        QuizId = quizId;
        PlayerId = playerId;
        QuestionCount = questionCount;
        ActivatedAt = activatedAt;
    }

    public void RecordAnswer(QuizAnswer answer, int quizXp)
    {
        if (SolvedAt is not null)
        {
            throw new DomainException("This quiz is already solved, so it does not accept new answers.");
        }

        _answers.Add(answer);

        if (_answers.Count == QuestionCount)
        {
            var correctCount = _answers.Count(recorded => recorded.IsCorrect);
            SolvedAt = answer.AnsweredAt;
            // Integer division rounds down, which is what the earned XP rule asks for.
            EarnedXp = quizXp * correctCount / QuestionCount;
        }
    }

    public int NextQuestionIndex()
    {
        return _answers.Count;
    }
}
