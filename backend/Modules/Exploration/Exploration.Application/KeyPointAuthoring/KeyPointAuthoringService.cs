using Exploration.Application.Tours;
using Exploration.Domain.Locations;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;

    public KeyPointAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddKeyPointAsync(Guid tourId, Guid authorId, KeyPointDataDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.AddKeyPoint(dto.Name, dto.Description, dto.ImageUrl, new Location(dto.Latitude, dto.Longitude),
            dto.Secret, dto.IsVisibleBeforePurchase);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeKeyPointAsync(Guid tourId, Guid authorId, Guid keyPointId, KeyPointDataDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.ChangeKeyPoint(keyPointId, dto.Name, dto.Description, dto.ImageUrl, new Location(dto.Latitude, dto.Longitude),
            dto.Secret, dto.IsVisibleBeforePurchase);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveKeyPointAsync(Guid tourId, Guid authorId, Guid keyPointId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.RemoveKeyPoint(keyPointId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MoveKeyPointAsync(Guid tourId, Guid authorId, Guid keyPointId, MoveKeyPointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.MoveKeyPoint(keyPointId, dto.Direction);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetCalculatedLengthAsync(Guid tourId, Guid authorId, TourLengthDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.SetCalculatedLength(dto.LengthKm);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
