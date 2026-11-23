using Identity.Infrastructure;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Lets another module's integration tests put the WellKnownUsers into the identity
/// schema, so that IIdentityApi finds them. The seed and the context stay internal;
/// only this entry point is public.
/// </summary>
public static class IdentityReseed
{
    public static void Apply(ExplorerApiFactory factory)
    {
        factory.Reseed<IdentityModuleDbContext>(IdentitySeed.All);
    }
}
