using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourPreferences;

public class PreferenceCriteriaTests
{
    [Fact]
    public void Criteria_without_any_difficulty_transport_mode_or_tag_is_rejected()
    {
        var creation = () => new PreferenceCriteria([], [], []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criteria_with_only_a_difficulty_is_valid()
    {
        var criteria = new PreferenceCriteria([TourDifficulty.Moderate], [], []);

        criteria.Difficulties.Should().Equal(TourDifficulty.Moderate);
    }

    [Fact]
    public void Criteria_with_only_a_transport_mode_is_valid()
    {
        var criteria = new PreferenceCriteria([], [TransportMode.Bicycle], []);

        criteria.TransportModes.Should().Equal(TransportMode.Bicycle);
    }

    [Fact]
    public void Criteria_with_only_a_tag_is_valid()
    {
        var criteria = new PreferenceCriteria([], [], ["istorija"]);

        criteria.Tags.Should().Equal("istorija");
    }

    [Theory]
    [InlineData("istorija", "istorija")]
    [InlineData("Istorija", "istorija")]
    public void Criteria_with_a_repeated_tag_is_rejected(string firstTag, string secondTag)
    {
        var creation = () => new PreferenceCriteria([], [], [firstTag, secondTag]);

        creation.Should().Throw<DomainException>();
    }
}
