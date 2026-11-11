using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private const string ValidName = "Jutarnje trčanje";
    private const int ValidRequiredParticipants = 5;
    private const int ValidRangeInMeters = 200;
    private const int ValidXp = 50;
    private const string ValidCompletionMessage = "Bravo!";
    private static readonly TimeOnly ValidStart = new(7, 0);
    private static readonly TimeSpan ValidDuration = TimeSpan.FromHours(1);

    private static SocialChallenge CreateChallenge() =>
        new(ValidName, ValidStart, ValidDuration, ValidRequiredParticipants, ValidRangeInMeters, ValidXp, ValidCompletionMessage);

    [Theory]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    [InlineData("   \t\n  ")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901")]
    public void Challenge_rejects_a_name_outside_the_allowed_length(string name)
    {
        var creation = () => new SocialChallenge(name, ValidStart, ValidDuration, ValidRequiredParticipants, ValidRangeInMeters, ValidXp, ValidCompletionMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(101)]
    public void Challenge_rejects_a_number_of_participants_outside_the_allowed_range(int requiredParticipants)
    {
        var creation = () => new SocialChallenge(ValidName, ValidStart, ValidDuration, requiredParticipants, ValidRangeInMeters, ValidXp, ValidCompletionMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(9)]
    [InlineData(1001)]
    public void Challenge_rejects_a_range_outside_the_allowed_meters(int rangeInMeters)
    {
        var creation = () => new SocialChallenge(ValidName, ValidStart, ValidDuration, ValidRequiredParticipants, rangeInMeters, ValidXp, ValidCompletionMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Challenge_rejects_xp_outside_the_allowed_range(int xp)
    {
        var creation = () => new SocialChallenge(ValidName, ValidStart, ValidDuration, ValidRequiredParticipants, ValidRangeInMeters, xp, ValidCompletionMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \t\n ")]
    public void Challenge_rejects_an_empty_completion_message(string completionMessage)
    {
        var creation = () => new SocialChallenge(ValidName, ValidStart, ValidDuration, ValidRequiredParticipants, ValidRangeInMeters, ValidXp, completionMessage);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_rejects_a_completion_message_longer_than_two_hundred_characters()
    {
        var completionMessage = new string('a', 201);

        var creation = () => new SocialChallenge(ValidName, ValidStart, ValidDuration, ValidRequiredParticipants, ValidRangeInMeters, ValidXp, completionMessage);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_with_all_lower_bounds_starts_as_a_draft()
    {
        var challenge = new SocialChallenge("abc", ValidStart, ValidDuration, 2, 10, 1, "a");

        challenge.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public void Challenge_with_all_upper_bounds_starts_as_a_draft()
    {
        var challenge = new SocialChallenge(new string('a', 60), ValidStart, ValidDuration, 100, 1000, 1000, new string('a', 200));

        challenge.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public void Challenge_keeps_its_name_and_completion_message_without_surrounding_whitespace()
    {
        var challenge = new SocialChallenge("  \tJutarnje trčanje\n ", ValidStart, ValidDuration, ValidRequiredParticipants, ValidRangeInMeters, ValidXp, " \nBravo!\t ");

        challenge.Name.Should().Be("Jutarnje trčanje");
        challenge.CompletionMessage.Should().Be("Bravo!");
    }

    [Theory]
    [InlineData("ab", 5, 200, 50, "Bravo!")]
    [InlineData(ValidName, 1, 200, 50, "Bravo!")]
    [InlineData(ValidName, 5, 9, 50, "Bravo!")]
    [InlineData(ValidName, 5, 200, 0, "Bravo!")]
    [InlineData(ValidName, 5, 200, 50, "   ")]
    public void Changing_a_challenge_rejects_invalid_data_like_creating_one(
        string name, int requiredParticipants, int rangeInMeters, int xp, string completionMessage)
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update(name, ValidStart, ValidDuration, requiredParticipants, rangeInMeters, xp, completionMessage);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Changing_a_challenge_rejects_an_invalid_time_window()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update(ValidName, ValidStart, TimeSpan.FromMinutes(14), ValidRequiredParticipants, ValidRangeInMeters, ValidXp, ValidCompletionMessage);

        update.Should().Throw<DomainException>();
    }
}
