using FluentAssertions;
using Games.Domain.Locations;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private const string ValidName = "Petrovaradinska tvrđava";
    private const string ValidInstructions = "Obiđite tvrđavu i odgovorite na pitanja.";
    private const int ValidXp = 50;
    private static readonly QuizLocation ValidLocation = new(new Location(45.2517, 19.8659), 50);

    private static List<Question> CreateQuestions(int count)
    {
        var questions = new List<Question>();
        for (var number = 1; number <= count; number++)
        {
            questions.Add(new Question($"Pitanje {number}", [new AnswerOption("Da", true), new AnswerOption("Ne", false)]));
        }
        return questions;
    }

    private static Quiz CreateQuizIn(QuizStatus status)
    {
        var quiz = new Quiz(ValidName, ValidInstructions, ValidXp, CreateQuestions(1), ValidLocation);
        if (status == QuizStatus.Draft)
        {
            return quiz;
        }
        quiz.Publish();
        if (status == QuizStatus.Published)
        {
            return quiz;
        }
        quiz.Retire();
        if (status == QuizStatus.Retired)
        {
            return quiz;
        }
        quiz.Archive();
        return quiz;
    }

    [Fact]
    public void Quiz_without_questions_cannot_be_created()
    {
        var creation = () => new Quiz(ValidName, ValidInstructions, ValidXp, [], null);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_more_than_fifteen_questions_cannot_be_created()
    {
        var creation = () => new Quiz(ValidName, ValidInstructions, ValidXp, CreateQuestions(16), null);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901")]
    public void Quiz_with_a_name_outside_the_allowed_length_cannot_be_created(string name)
    {
        var creation = () => new Quiz(name, ValidInstructions, ValidXp, CreateQuestions(1), null);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \t\n ")]
    public void Quiz_with_empty_instructions_cannot_be_created(string instructions)
    {
        var creation = () => new Quiz(ValidName, instructions, ValidXp, CreateQuestions(1), null);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_instructions_longer_than_one_thousand_characters_cannot_be_created()
    {
        var creation = () => new Quiz(ValidName, new string('a', 1001), ValidXp, CreateQuestions(1), null);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Quiz_with_xp_outside_the_allowed_range_cannot_be_created(int xp)
    {
        var creation = () => new Quiz(ValidName, ValidInstructions, xp, CreateQuestions(1), null);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_all_lower_bounds_starts_as_a_draft()
    {
        var quiz = new Quiz("abc", "a", 1, CreateQuestions(1), null);

        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Quiz_with_all_upper_bounds_starts_as_a_draft()
    {
        var quiz = new Quiz(new string('a', 60), new string('a', 1000), 1000, CreateQuestions(15), null);

        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Draft_quiz_is_published()
    {
        var quiz = CreateQuizIn(QuizStatus.Draft);

        quiz.Publish();

        quiz.Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public void Quiz_without_a_location_cannot_be_published_and_stays_a_draft()
    {
        var quiz = new Quiz(ValidName, ValidInstructions, ValidXp, CreateQuestions(1), null);

        var publishing = () => quiz.Publish();

        publishing.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_a_draft_cannot_be_published_and_keeps_its_state(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var publishing = () => quiz.Publish();

        publishing.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Fact]
    public void Published_quiz_is_retired()
    {
        var quiz = CreateQuizIn(QuizStatus.Published);

        quiz.Retire();

        quiz.Status.Should().Be(QuizStatus.Retired);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_published_cannot_be_retired_and_keeps_its_state(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var retiring = () => quiz.Retire();

        retiring.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Fact]
    public void Retired_quiz_is_archived()
    {
        var quiz = CreateQuizIn(QuizStatus.Retired);

        quiz.Archive();

        quiz.Status.Should().Be(QuizStatus.Archived);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_retired_cannot_be_archived_and_keeps_its_state(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var archiving = () => quiz.Archive();

        archiving.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Retired_or_archived_quiz_is_repeated_as_a_draft(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        quiz.Repeat();

        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Published)]
    public void Draft_or_published_quiz_cannot_be_repeated_and_keeps_its_state(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var repeating = () => quiz.Repeat();

        repeating.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Archived)]
    public void Draft_or_archived_quiz_can_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var deletion = () => quiz.EnsureCanBeDeleted();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    public void Published_or_retired_quiz_cannot_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var deletion = () => quiz.EnsureCanBeDeleted();

        deletion.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_a_draft_cannot_be_changed(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var update = () => quiz.Update(ValidName, ValidInstructions, ValidXp, CreateQuestions(1), ValidLocation);

        update.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void Answer_to_a_question_the_quiz_does_not_have_cannot_be_checked(int questionIndex)
    {
        var quiz = CreateQuizIn(QuizStatus.Published);

        var checking = () => quiz.IsCorrect(questionIndex, 0);

        checking.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Answer_option_the_question_does_not_have_cannot_be_checked(int optionIndex)
    {
        var quiz = CreateQuizIn(QuizStatus.Published);

        var checking = () => quiz.IsCorrect(0, optionIndex);

        checking.Should().Throw<DomainException>();
    }
}
