using Shared.Domain;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
    Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize);
}
