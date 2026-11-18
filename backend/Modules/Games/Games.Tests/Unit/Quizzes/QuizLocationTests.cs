using FluentAssertions;
using Games.Domain.Locations;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizLocationTests
{
    private static readonly Location ValidLocation = new(45.2551, 19.8451);

    [Theory]
    [InlineData(9)]
    [InlineData(1001)]
    public void Location_with_a_range_just_outside_the_allowed_bounds_cannot_be_created(int rangeInMeters)
    {
        var creation = () => new QuizLocation(ValidLocation, rangeInMeters);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_without_coordinates_cannot_be_created()
    {
        var creation = () => new QuizLocation(null!, 50);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public void Location_with_a_range_exactly_on_the_bounds_is_created(int rangeInMeters)
    {
        var location = new QuizLocation(ValidLocation, rangeInMeters);

        location.Location.Should().Be(ValidLocation);
        location.RangeInMeters.Should().Be(rangeInMeters);
    }
}
