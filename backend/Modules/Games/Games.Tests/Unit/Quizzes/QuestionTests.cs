using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuestionTests
{
    private const string ValidText = "Ko je projektovao tvrđavu?";

    private static List<AnswerOption> CreateOptions(int count)
    {
        var options = new List<AnswerOption> { new("Odgovor 1", true) };
        for (var number = 2; number <= count; number++)
        {
            options.Add(new AnswerOption($"Odgovor {number}", false));
        }
        return options;
    }

    [Fact]
    public void Question_with_a_single_answer_option_cannot_be_created()
    {
        var creation = () => new Question(ValidText, CreateOptions(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Question_with_more_than_six_answer_options_cannot_be_created()
    {
        var creation = () => new Question(ValidText, CreateOptions(7));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)]
    public void Question_accepts_a_number_of_answer_options_at_the_edge_of_the_allowed_range(int count)
    {
        var question = new Question(ValidText, CreateOptions(count));

        question.Options.Should().HaveCount(count);
    }

    [Fact]
    public void Question_without_a_correct_answer_option_cannot_be_created()
    {
        var options = new List<AnswerOption> { new("Vauban", false), new("Savojski", false) };

        var creation = () => new Question(ValidText, options);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Question_with_two_correct_answer_options_cannot_be_created()
    {
        var options = new List<AnswerOption> { new("Vauban", true), new("Savojski", true) };

        var creation = () => new Question(ValidText, options);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \t\n ")]
    public void Question_with_an_empty_text_cannot_be_created(string text)
    {
        var creation = () => new Question(text, CreateOptions(2));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Question_with_a_text_longer_than_two_hundred_characters_cannot_be_created()
    {
        var creation = () => new Question(new string('a', 201), CreateOptions(2));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Question_accepts_a_text_of_exactly_two_hundred_characters()
    {
        var text = new string('a', 200);

        var question = new Question(text, CreateOptions(2));

        question.Text.Should().Be(text);
    }

    [Fact]
    public void Question_with_two_answer_options_that_differ_only_in_letter_case_and_surrounding_whitespace_cannot_be_created()
    {
        var options = new List<AnswerOption> { new("Vauban", true), new("  vAUBAN \t", false) };

        var creation = () => new Question(ValidText, options);

        creation.Should().Throw<DomainException>();
    }
}
