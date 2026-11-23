using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPublishing;

public sealed class TourPublishingService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourPublishingService(ITourRepository tourRepository, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReactivateAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Reactivate();
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
