namespace Positions.Tests.Integration.Seeds;

internal static class PositionsSeed
{
    public static object[] All => [.. UserPositionSeed.All];
}
