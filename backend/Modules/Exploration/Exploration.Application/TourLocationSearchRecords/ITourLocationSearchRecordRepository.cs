using Exploration.Domain.TourLocationSearchRecords;

namespace Exploration.Application.TourLocationSearchRecords;

public interface ITourLocationSearchRecordRepository
{
    void Add(TourLocationSearchRecord record);
}
