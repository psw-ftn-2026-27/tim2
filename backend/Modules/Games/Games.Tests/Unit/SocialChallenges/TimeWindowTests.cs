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

    // On 28 March 2027 Belgrade skips from 02:00 to 03:00 (01:00 UTC), so 02:30 does not exist that day.
    [Fact]
    public void Window_starting_in_the_skipped_spring_hour_opens_when_the_clock_jumps_forward()
    {
        var window = new TimeWindow(new TimeOnly(2, 30), TimeSpan.FromHours(1));
        var clockJump = new DateTime(2027, 3, 28, 1, 0, 0, DateTimeKind.Utc);

        var end = window.EndAfter(clockJump);

        end.Should().Be(clockJump.AddHours(1));
    }

    [Theory]
    [InlineData(0, 59, false)]
    [InlineData(1, 0, true)]
    [InlineData(1, 59, true)]
    [InlineData(2, 0, false)]
    public void Window_starting_in_the_skipped_spring_hour_is_open_for_its_duration_from_the_clock_jump(int utcHour, int utcMinute, bool expectedOpen)
    {
        var window = new TimeWindow(new TimeOnly(2, 30), TimeSpan.FromHours(1));

        var isOpen = window.Contains(new DateTime(2027, 3, 28, utcHour, utcMinute, 0, DateTimeKind.Utc));

        isOpen.Should().Be(expectedOpen);
    }

    // On 25 October 2026 Belgrade repeats the hour from 02:00 to 03:00: first in summer time (00:00 to 01:00 UTC),
    // then in winter time (01:00 to 02:00 UTC).
    [Theory]
    [InlineData(0, 29, false)]
    [InlineData(0, 30, true)]
    [InlineData(1, 29, true)]
    [InlineData(1, 30, false)]
    public void Window_starting_in_the_repeated_autumn_hour_opens_at_its_first_occurrence(int utcHour, int utcMinute, bool expectedOpen)
    {
        var window = new TimeWindow(new TimeOnly(2, 30), TimeSpan.FromHours(1));

        var isOpen = window.Contains(new DateTime(2026, 10, 25, utcHour, utcMinute, 0, DateTimeKind.Utc));

        isOpen.Should().Be(expectedOpen);
    }
}
