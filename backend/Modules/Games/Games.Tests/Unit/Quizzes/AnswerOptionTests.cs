using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class AnswerOptionTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  \t\n ")]
    public void Answer_option_with_an_empty_text_cannot_be_created(string text)
    {
        var creation = () => new AnswerOption(text, true);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Answer_option_with_a_text_longer_than_one_hundred_characters_cannot_be_created()
    {
        var text = new string('a', 101);

        var creation = () => new AnswerOption(text, true);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Answer_option_accepts_a_text_of_exactly_one_hundred_characters()
    {
        var text = new string('a', 100);

        var option = new AnswerOption(text, true);

        option.Text.Should().Be(text);
    }
}
