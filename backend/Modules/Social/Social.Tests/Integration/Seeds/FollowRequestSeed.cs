using Shared.Tests;
using Social.Domain.FollowRequests;

namespace Social.Tests.Integration.Seeds;

internal static class FollowRequestSeed
{
    public static readonly Guid FormerlyFollowedUser = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
    public static readonly Guid RejectingUser = Guid.Parse("00000000-0000-0000-0000-0000000000f2");
    public static readonly Guid FollowedUser = Guid.Parse("00000000-0000-0000-0000-0000000000f3");

    public static readonly FollowRequest ExplorerToSecondExplorerPending;
    public static readonly FollowRequest SecondExplorerToAdministratorPending;
    public static readonly FollowRequest AdministratorRemovedByExplorer;
    public static readonly FollowRequest ExplorerRejectedByFormerlyFollowedUser;
    public static readonly FollowRequest ExplorerRemovedByFormerlyFollowedUser;
    public static readonly FollowRequest ExplorerRejectedByRejectingUser;
    public static readonly FollowRequest ExplorerAcceptedByFollowedUser;

    static FollowRequestSeed()
    {
        ExplorerToSecondExplorerPending = new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));

        SecondExplorerToAdministratorPending = new(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));

        AdministratorRemovedByExplorer = new(WellKnownUsers.Administrator, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        AdministratorRemovedByExplorer.AcceptAutomatically();
        AdministratorRemovedByExplorer.RemoveFollower(WellKnownUsers.Explorer);

        ExplorerRejectedByFormerlyFollowedUser = new(WellKnownUsers.Explorer, FormerlyFollowedUser,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        ExplorerRejectedByFormerlyFollowedUser.Reject(FormerlyFollowedUser);

        ExplorerRemovedByFormerlyFollowedUser = new(WellKnownUsers.Explorer, FormerlyFollowedUser,
            new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc));
        ExplorerRemovedByFormerlyFollowedUser.AcceptAutomatically();
        ExplorerRemovedByFormerlyFollowedUser.RemoveFollower(FormerlyFollowedUser);

        ExplorerRejectedByRejectingUser = new(WellKnownUsers.Explorer, RejectingUser,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        ExplorerRejectedByRejectingUser.Reject(RejectingUser);

        ExplorerAcceptedByFollowedUser = new(WellKnownUsers.Explorer, FollowedUser,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        ExplorerAcceptedByFollowedUser.AcceptAutomatically();
    }

    public static FollowRequest[] All =>
    [
        ExplorerToSecondExplorerPending,
        SecondExplorerToAdministratorPending,
        AdministratorRemovedByExplorer,
        ExplorerRejectedByFormerlyFollowedUser,
        ExplorerRemovedByFormerlyFollowedUser,
        ExplorerRejectedByRejectingUser,
        ExplorerAcceptedByFollowedUser
    ];
}
