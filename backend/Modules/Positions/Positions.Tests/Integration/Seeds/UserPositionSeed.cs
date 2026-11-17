using Positions.Domain.UserPositions;
using Shared.Tests;

namespace Positions.Tests.Integration.Seeds;

internal static class UserPositionSeed
{
    public static readonly UserPosition ExplorersPositionInNoviSad;
    public static readonly UserPosition SecondExplorersPositionInBelgrade;

    static UserPositionSeed()
    {
        ExplorersPositionInNoviSad = new(WellKnownUsers.Explorer, new Coordinates(45.2671, 19.8335));
        SecondExplorersPositionInBelgrade = new(WellKnownUsers.SecondExplorer, new Coordinates(44.8176, 20.4569));
    }

    public static UserPosition[] All => [ExplorersPositionInNoviSad, SecondExplorersPositionInBelgrade];
}
