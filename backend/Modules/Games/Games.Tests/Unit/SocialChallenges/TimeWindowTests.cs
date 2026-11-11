using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class TimeWindowTests
{
    [Theory]
    [InlineData(14)]
    [InlineData(24 * 60)]
    public void Window_rejects_a_duration_outside_the_allowed_range(int minutes)
    {
        var creation = () => new TimeWindow(new TimeOnly(10, 0), TimeSpan.FromMinutes(minutes));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(15)]
    [InlineData(23 * 60 + 59)]
    public void Window_accepts_a_duration_at_the_edge_of_the_allowed_range(int minutes)
    {
        var window = new TimeWindow(new TimeOnly(10, 0), TimeSpan.FromMinutes(minutes));

        window.Duration.Should().Be(TimeSpan.FromMinutes(minutes));
    }

    [Fact]
    public void Window_that_starts_at_ten_in_the_evening_and_lasts_four_hours_ends_at_two_in_the_morning()
    {
        var window = new TimeWindow(new TimeOnly(22, 0), TimeSpan.FromHours(4));

        var end = window.End();

        end.Should().Be(new TimeOnly(2, 0));
    }
}
