using Exploration.Infrastructure.Persistence;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

/// <summary>
/// Lets another module's integration tests put the Exploration seeds into the exploration
/// schema, so that IExplorationApi finds them. The seeds and the context stay internal;
/// only this entry point is public.
/// </summary>
public static class ExplorationReseed
{
    public static void Apply(ExplorerApiFactory factory)
    {
        factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
    }
}
