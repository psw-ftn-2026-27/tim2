using Identity.Contracts;
using Identity.Core;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

internal sealed class IdentityApi : IIdentityApi
{
    private const string ExplorerRole = "explorer";

    private readonly IdentityModuleDbContext _dbContext;

    public IdentityApi(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExplorerPage> GetExplorersAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var explorers = Explorers();

        var totalCount = await explorers.CountAsync();
        var items = await explorers
            .OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new ExplorerSummary(user.Id, user.Email!))
            .ToListAsync();

        return new ExplorerPage(items, totalCount);
    }

    public Task<ExplorerSummary?> GetExplorerAsync(Guid userId)
    {
        return Explorers()
            .Where(user => user.Id == userId)
            .Select(user => new ExplorerSummary(user.Id, user.Email!))
            .FirstOrDefaultAsync();
    }

    private IQueryable<ApplicationUser> Explorers()
    {
        return from user in _dbContext.Users
               join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
               join role in _dbContext.Roles on userRole.RoleId equals role.Id
               where role.Name == ExplorerRole
               select user;
    }
}
