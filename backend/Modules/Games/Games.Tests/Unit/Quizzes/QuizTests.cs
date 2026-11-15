using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private const string ValidName = "Petrovaradinska tvrđava";
    private const string ValidInstructions = "Obiđite tvrđavu i odgovorite na pitanja.";
    private const int ValidXp = 50;

    private static List<Question> CreateQuestions(int count)
    {
        var questions = new List<Question>();
        for (var number = 1; number <= count; number++)
        {
            questions.Add(new Question($"Pitanje {number}", [new AnswerOption("Da", true), new AnswerOption("Ne", false)]));
        }
        return questions;
    }

    [Fact]
    public void Quiz_without_questions_cannot_be_created()
    {
        var creation = () => new Quiz(ValidName, ValidInstructions, ValidXp, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_more_than_twenty_questions_cannot_be_created()
    {
        var creation = () => new Quiz(ValidName, ValidInstructions, ValidXp, CreateQuestions(21));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901")]
    public void Quiz_with_a_name_outside_the_allowed_length_cannot_be_created(string name)
    {
        var creation = () => new Quiz(name, ValidInstructions, ValidXp, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \t\n ")]
    public void Quiz_with_empty_instructions_cannot_be_created(string instructions)
    {
        var creation = () => new Quiz(ValidName, instructions, ValidXp, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_instructions_longer_than_one_thousand_characters_cannot_be_created()
    {
        var creation = () => new Quiz(ValidName, new string('a', 1001), ValidXp, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Quiz_with_xp_outside_the_allowed_range_cannot_be_created(int xp)
    {
        var creation = () => new Quiz(ValidName, ValidInstructions, xp, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_all_lower_bounds_starts_as_a_draft()
    {
        var quiz = new Quiz("abc", "a", 1, CreateQuestions(1));

        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Quiz_with_all_upper_bounds_starts_as_a_draft()
    {
        var quiz = new Quiz(new string('a', 60), new string('a', 1000), 1000, CreateQuestions(20));

        quiz.Status.Should().Be(QuizStatus.Draft);
    }
}
