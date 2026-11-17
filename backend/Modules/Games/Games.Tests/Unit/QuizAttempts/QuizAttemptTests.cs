using FluentAssertions;
using Games.Domain.QuizAttempts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public class QuizAttemptTests
{
    private static readonly DateTime ActivatedAt = new(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

    private static QuizAttempt CreateAttempt(int questionCount)
    {
        return new QuizAttempt(Guid.NewGuid(), Guid.NewGuid(), questionCount, ActivatedAt);
    }

    private static QuizAnswer CreateAnswer(bool isCorrect, int minutesAfterActivation)
    {
        return new QuizAnswer(0, isCorrect, ActivatedAt.AddMinutes(minutesAfterActivation));
    }

    [Fact]
    public void Attempt_is_solved_after_the_last_answer_and_earns_the_quiz_xp_share_of_correct_answers_rounded_down()
    {
        var attempt = CreateAttempt(3);
        attempt.RecordAnswer(CreateAnswer(true, 1), 100);
        attempt.RecordAnswer(CreateAnswer(false, 2), 100);
        var lastAnswer = CreateAnswer(true, 3);

        attempt.RecordAnswer(lastAnswer, 100);

        attempt.SolvedAt.Should().Be(lastAnswer.AnsweredAt);
        attempt.EarnedXp.Should().Be(66);
    }

    [Fact]
    public void Attempt_without_a_single_correct_answer_is_solved_with_no_xp()
    {
        var attempt = CreateAttempt(2);
        attempt.RecordAnswer(CreateAnswer(false, 1), 100);

        attempt.RecordAnswer(CreateAnswer(false, 2), 100);

        attempt.SolvedAt.Should().NotBeNull();
        attempt.EarnedXp.Should().Be(0);
    }

    [Fact]
    public void Solved_attempt_does_not_accept_a_new_answer()
    {
        var attempt = CreateAttempt(1);
        attempt.RecordAnswer(CreateAnswer(true, 1), 100);

        var answering = () => attempt.RecordAnswer(CreateAnswer(true, 2), 100);

        answering.Should().Throw<DomainException>();
        attempt.Answers.Should().HaveCount(1);
    }
}
