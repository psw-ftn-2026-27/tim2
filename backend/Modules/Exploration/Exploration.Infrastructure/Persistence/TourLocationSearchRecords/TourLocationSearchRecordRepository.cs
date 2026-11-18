using Exploration.Application.TourLocationSearchRecords;
using Exploration.Domain.TourLocationSearchRecords;

namespace Exploration.Infrastructure.Persistence.TourLocationSearchRecords;

internal sealed class TourLocationSearchRecordRepository : ITourLocationSearchRecordRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourLocationSearchRecordRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(TourLocationSearchRecord record)
    {
        _dbContext.TourLocationSearchRecords.Add(record);
    }
}
