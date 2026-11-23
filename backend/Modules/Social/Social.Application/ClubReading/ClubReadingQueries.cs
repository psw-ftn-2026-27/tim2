using Shared.Domain;
using Social.Application.Clubs;

namespace Social.Application.ClubReading;

public sealed class ClubReadingQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubReadingQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _clubReadRepository.GetAllAsync(page, pageSize);
    }
}
