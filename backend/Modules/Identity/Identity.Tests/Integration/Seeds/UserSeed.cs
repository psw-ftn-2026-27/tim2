using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Unlike feature-module seeds, these rows cannot come from a domain constructor:
/// Reseed inserts them straight into the database, bypassing UserManager, so the
/// normalized columns and the password hash must be filled in here.
/// </summary>
internal static class UserSeed
{
    public const string Password = "SuperSecret1!";

    private static readonly PasswordHasher<ApplicationUser> Hasher = new();

    public static readonly ApplicationUser Explorer = new()
    {
        Id = WellKnownUsers.Explorer,
        UserName = "explorer@test.com",
        NormalizedUserName = "EXPLORER@TEST.COM",
        Email = "explorer@test.com",
        NormalizedEmail = "EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5f4a2f3e-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly IdentityUserRole<Guid> ExplorerRole = new() { UserId = WellKnownUsers.Explorer, RoleId = RoleSeed.Explorer.Id };

    public static readonly ApplicationUser SecondExplorer = new()
    {
        Id = WellKnownUsers.SecondExplorer,
        UserName = "second-explorer@test.com",
        NormalizedUserName = "SECOND-EXPLORER@TEST.COM",
        Email = "second-explorer@test.com",
        NormalizedEmail = "SECOND-EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "8c3b6d1a-2e4f-4a7b-9c0d-1e2f3a4b5c6d"
    };

    public static readonly IdentityUserRole<Guid> SecondExplorerRole = new() { UserId = WellKnownUsers.SecondExplorer, RoleId = RoleSeed.Explorer.Id };

    public static readonly ApplicationUser Administrator = new()
    {
        Id = WellKnownUsers.Administrator,
        UserName = "administrator@test.com",
        NormalizedUserName = "ADMINISTRATOR@TEST.COM",
        Email = "administrator@test.com",
        NormalizedEmail = "ADMINISTRATOR@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d"
    };

    public static readonly IdentityUserRole<Guid> AdministratorRole = new() { UserId = WellKnownUsers.Administrator, RoleId = RoleSeed.Administrator.Id };

    public static ApplicationUser[] Explorers => [Explorer, SecondExplorer];

    public static object[] All => [Explorer, ExplorerRole, SecondExplorer, SecondExplorerRole, Administrator, AdministratorRole];
}
