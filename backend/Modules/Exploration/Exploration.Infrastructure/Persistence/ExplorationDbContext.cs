using Exploration.Application;
using Exploration.Domain.TourLocationSearchRecords;
using Exploration.Domain.TourPreferences;
using Exploration.Domain.TourSessions;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence.TourLocationSearchRecords;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.TourSessions;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationDbContext : DbContext, IUnitOfWork
{
    public ExplorationDbContext(DbContextOptions<ExplorationDbContext> options) : base(options) { }

    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourPreference> TourPreferences => Set<TourPreference>();
    public DbSet<TourLocationSearchRecord> TourLocationSearchRecords => Set<TourLocationSearchRecord>();
    public DbSet<TourSession> TourSessions => Set<TourSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("exploration");
        builder.ConfigureTours();
        builder.ConfigureTourPreferences();
        builder.ConfigureTourLocationSearchRecords();
        builder.ConfigureTourSessions();
    }
}
