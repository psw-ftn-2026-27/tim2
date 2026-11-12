using AutoMapper;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TourAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TourDto> CreateAsync(Guid authorId, CreateTourDto dto)
    {
        var tour = new Tour(authorId, dto.Name, dto.Description, dto.Difficulty, dto.Tags);
        if (dto.RecommendedGroupSize is not null)
        {
            tour.ChangeRecommendedGroupSize(dto.RecommendedGroupSize.Min, dto.RecommendedGroupSize.Max);
        }
        if (dto.Price != 0)
        {
            tour.ChangePrice(dto.Price);
        }

        _tourRepository.Add(tour);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TourDto>(tour);
    }

    public async Task AddTransportTimeAsync(Guid tourId, Guid authorId, TransportTimeDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.AddTransportTime(dto.Transport, dto.Minutes);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeRecommendedGroupSizeAsync(Guid tourId, Guid authorId, RecommendedGroupSizeDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.ChangeRecommendedGroupSize(dto.Min, dto.Max);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePriceAsync(Guid tourId, Guid authorId, TourPriceDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.ChangePrice(dto.Price);
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
