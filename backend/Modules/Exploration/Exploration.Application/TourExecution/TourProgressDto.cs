using Exploration.Domain.TourSessions;

namespace Exploration.Application.TourExecution;

public sealed record TourProgressDto(
    string TourName,
    TourSessionStatus Status,
    TourProgressKeyPointDto? NextKeyPoint,
    List<CompletedKeyPointDto> CompletedKeyPoints);
