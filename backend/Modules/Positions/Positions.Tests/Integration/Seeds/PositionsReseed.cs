using Positions.Infrastructure.Persistence;
using Shared.Tests;

namespace Positions.Tests.Integration.Seeds;

/// <summary>
/// Lets another module's integration tests put the Positions seeds into the positions
/// schema, so that IPositionsApi finds them. The seeds and the context are internal;
/// Games.Tests reads the seeded positions through InternalsVisibleTo in Positions.Tests.csproj,
/// so a test never prepares a position through PUT /api/positions/mine.
/// </summary>
public static class PositionsReseed
{
    public static void Apply(ExplorerApiFactory factory)
    {
        factory.Reseed<PositionsDbContext>(PositionsSeed.All);
    }
}
