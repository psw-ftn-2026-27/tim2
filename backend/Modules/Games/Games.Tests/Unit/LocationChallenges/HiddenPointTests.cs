using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class HiddenPointTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  \t\n ")]
    public void Hidden_point_without_a_photo_cannot_exist(string imageUrl)
    {
        var creation = () => new HiddenPoint(imageUrl, new Location(45.2551, 19.8451));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Hidden_point_without_a_location_cannot_exist()
    {
        var creation = () => new HiddenPoint("/uploads/tacka.jpg", null!);

        creation.Should().Throw<DomainException>();
    }
}
