namespace Exploration.Application.TourExecution;

public sealed record ActiveTourDto(
    string TourName,
    ActiveTourKeyPointDto NextKeyPoint);
