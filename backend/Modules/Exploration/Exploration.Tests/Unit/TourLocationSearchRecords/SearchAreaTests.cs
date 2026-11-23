using Exploration.Domain.Locations;
using Exploration.Domain.TourLocationSearchRecords;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourLocationSearchRecords;

public class SearchAreaTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    public void Search_area_with_a_radius_that_is_not_positive_is_rejected(double radiusKm)
    {
        var creation = () => new SearchArea(new Location(44.8176, 20.4569), radiusKm);

        creation.Should().Throw<DomainException>();
    }
}
