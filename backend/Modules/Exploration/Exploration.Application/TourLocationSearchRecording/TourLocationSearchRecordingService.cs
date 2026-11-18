using Exploration.Application.TourLocationSearchRecords;
using Exploration.Domain.Locations;
using Exploration.Domain.TourLocationSearchRecords;

namespace Exploration.Application.TourLocationSearchRecording;

public sealed class TourLocationSearchRecordingService
{
    private readonly ITourLocationSearchRecordRepository _recordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourLocationSearchRecordingService(ITourLocationSearchRecordRepository recordRepository, IUnitOfWork unitOfWork)
    {
        _recordRepository = recordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SaveLocationSearchAsync(LocationSearchRecordDto dto)
    {
        var area = new SearchArea(new Location(dto.Latitude, dto.Longitude), dto.RadiusKm);
        var record = new TourLocationSearchRecord(area, dto.Name, dto.Difficulties, dto.Tags, dto.TransportModes,
            dto.MinPrice, dto.MaxPrice, dto.MinLengthKm, dto.MaxLengthKm, dto.ResultCount);

        _recordRepository.Add(record);
        await _unitOfWork.SaveChangesAsync();

        return record.Id;
    }
}
