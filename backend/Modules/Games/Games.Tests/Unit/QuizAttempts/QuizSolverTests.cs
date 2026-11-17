using FluentAssertions;
using Games.Domain.Locations;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public class QuizSolverTests
{
    private const double QuizLatitude = 45;
    private const double QuizLongitude = 19;
    private const int QuizRangeInMeters = 100;
    private static readonly DateTime Now = new(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

    private readonly QuizSolver _solver = new();
    private readonly Guid _playerId = Guid.NewGuid();

    private static List<Question> CreateQuestions(int count)
    {
        var questions = new List<Question>();
        for (var number = 1; number <= count; number++)
        {
            questions.Add(new Question($"Pitanje {number}", [new AnswerOption("Da", true), new AnswerOption("Ne", false)]));
        }
        return questions;
    }

    private static Quiz CreateDraftQuiz(int questionCount)
    {
        return new Quiz(
            "Petrovaradinska tvrđava",
            "Obiđite tvrđavu i odgovorite na pitanja.",
            100,
            CreateQuestions(questionCount),
            new QuizLocation(new Location(QuizLatitude, QuizLongitude), QuizRangeInMeters));
    }

    private static Quiz CreatePublishedQuiz(int questionCount)
    {
        var quiz = CreateDraftQuiz(questionCount);
        quiz.Publish();
        return quiz;
    }

    private QuizAttempt Activate(Quiz quiz)
    {
        return _solver.Activate(quiz, null, QuizLatitude, QuizLongitude, _playerId, Now);
    }

    [Fact]
    public void Player_exactly_on_the_range_boundary_activates_the_quiz()
    {
        var quiz = CreatePublishedQuiz(2);

        // About 100 m north of the quiz location, which is exactly the range.
        var attempt = _solver.Activate(quiz, null, 45.0008993216, QuizLongitude, _playerId, Now);

        attempt.QuizId.Should().Be(quiz.Id);
        attempt.PlayerId.Should().Be(_playerId);
        attempt.QuestionCount.Should().Be(2);
    }

    [Fact]
    public void Player_one_meter_beyond_the_range_boundary_cannot_activate_the_quiz()
    {
        var quiz = CreatePublishedQuiz(2);

        // About 101 m north of the quiz location.
        var activation = () => _solver.Activate(quiz, null, 45.0009083148, QuizLongitude, _playerId, Now);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Player_who_already_has_an_attempt_cannot_activate_the_quiz_again()
    {
        var quiz = CreatePublishedQuiz(2);
        var existingAttempt = Activate(quiz);

        var activation = () => _solver.Activate(quiz, existingAttempt, QuizLatitude, QuizLongitude, _playerId, Now);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_that_is_not_published_cannot_be_activated()
    {
        var quiz = CreateDraftQuiz(2);

        var activation = () => Activate(quiz);

        activation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public void Answer_for_a_question_other_than_the_current_one_is_rejected(int expectedQuestionIndex)
    {
        var quiz = CreatePublishedQuiz(2);
        var attempt = Activate(quiz);

        var answering = () => _solver.Answer(quiz, attempt, expectedQuestionIndex, 0, Now);

        answering.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public void Answer_is_rejected_when_the_number_of_quiz_questions_changed_after_activation()
    {
        var quiz = CreatePublishedQuiz(2);
        var attempt = Activate(quiz);
        quiz.Retire();
        quiz.Repeat();
        quiz.Update(quiz.Name, quiz.Instructions, quiz.Xp, CreateQuestions(3), quiz.Location);

        var answering = () => _solver.Answer(quiz, attempt, 0, 0, Now);

        answering.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public void Player_can_answer_after_the_quiz_is_retired()
    {
        var quiz = CreatePublishedQuiz(2);
        var attempt = Activate(quiz);
        quiz.Retire();

        _solver.Answer(quiz, attempt, 0, 0, Now);

        attempt.Answers.Should().ContainSingle().Which.Should().Be(new QuizAnswer(0, true, Now));
    }
}
