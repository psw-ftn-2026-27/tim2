using Games.Application.LocationChallenges;
using Games.Application.Locations;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeManagement;

public sealed class LocationChallengeManagementService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeManagementService(ILocationChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateLocationChallengeDto dto)
    {
        var challenge = new LocationChallenge(dto.Name, dto.Xp, ToLocation(dto.StartLocation), ToHiddenPoints(dto.HiddenPoints));

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    public async Task UpdateAsync(Guid challengeId, UpdateLocationChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Update(dto.Name, dto.Xp, ToLocation(dto.StartLocation), ToHiddenPoints(dto.HiddenPoints));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.EnsureCanBeDeleted();
        _challengeRepository.Remove(challenge);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LocationChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {challengeId} does not exist.");
        }
        return challenge;
    }

    private static Location ToLocation(LocationDto location)
    {
        return new Location(location.Latitude, location.Longitude);
    }

    private static List<HiddenPoint> ToHiddenPoints(List<HiddenPointDto>? hiddenPoints)
    {
        return (hiddenPoints ?? [])
            .Select(point => new HiddenPoint(point.ImageUrl, ToLocation(point.Location)))
            .ToList();
    }
}
