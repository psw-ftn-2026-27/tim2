using AutoMapper;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubAuthoring;

public sealed class ClubAuthoringService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClubAuthoringService(IClubRepository clubRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ClubDto> CreateAsync(Guid ownerId, CreateClubDto dto)
    {
        var club = new Club(ownerId, dto.Name, dto.Description, dto.Images);

        _clubRepository.Add(club);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ClubDto>(club);
    }

    public async Task UpdateAsync(Guid clubId, Guid userId, UpdateClubDto dto)
    {
        var club = await GetClubAsync(clubId);

        club.Update(userId, dto.Name, dto.Description, dto.Images);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.EnsureOwnedBy(userId);
        _clubRepository.Remove(club);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
