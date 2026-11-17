using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizLocationTests
{
    [Theory]
    [InlineData(-90.0001, 0, 50)]
    [InlineData(90.0001, 0, 50)]
    [InlineData(0, -180.0001, 50)]
    [InlineData(0, 180.0001, 50)]
    [InlineData(0, 0, 9)]
    [InlineData(0, 0, 1001)]
    public void Location_with_a_coordinate_or_range_just_outside_the_allowed_bounds_cannot_be_created(double latitude, double longitude, int rangeInMeters)
    {
        var creation = () => new QuizLocation(latitude, longitude, rangeInMeters);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.NaN)]
    public void Location_with_a_coordinate_that_is_not_a_number_cannot_be_created(double latitude, double longitude)
    {
        var creation = () => new QuizLocation(latitude, longitude, 50);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90, -180, 10)]
    [InlineData(90, 180, 1000)]
    public void Location_with_coordinates_and_range_exactly_on_the_bounds_is_created(double latitude, double longitude, int rangeInMeters)
    {
        var location = new QuizLocation(latitude, longitude, rangeInMeters);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
        location.RangeInMeters.Should().Be(rangeInMeters);
    }
}
