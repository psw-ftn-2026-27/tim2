using Microsoft.EntityFrameworkCore;
using Positions.Application;
using Positions.Domain.UserPositions;
using Positions.Infrastructure.Persistence.UserPositions;

namespace Positions.Infrastructure.Persistence;

internal sealed class PositionsDbContext : DbContext, IUnitOfWork
{
    public PositionsDbContext(DbContextOptions<PositionsDbContext> options) : base(options) { }

    public DbSet<UserPosition> UserPositions => Set<UserPosition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("positions");
        builder.ConfigureUserPositions();
    }
}
