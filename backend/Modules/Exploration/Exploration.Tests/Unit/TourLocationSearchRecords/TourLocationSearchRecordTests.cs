using Exploration.Domain.Locations;
using Exploration.Domain.TourLocationSearchRecords;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourLocationSearchRecords;

public class TourLocationSearchRecordTests
{
    [Fact]
    public void Record_of_a_search_that_found_no_tours_is_created()
    {
        var record = CreateRecord(0);

        record.ResultCount.Should().Be(0);
    }

    [Fact]
    public void Record_of_a_search_with_a_negative_number_of_found_tours_is_not_created()
    {
        var creation = () => CreateRecord(-1);

        creation.Should().Throw<DomainException>();
    }

    private static TourLocationSearchRecord CreateRecord(int resultCount)
    {
        return new TourLocationSearchRecord(new SearchArea(new Location(45.2551, 19.8452), 10),
            null, null, null, null, null, null, null, null, resultCount);
    }
}
