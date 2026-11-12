using Shared.Tests;
using Social.Domain.FollowRequests;

namespace Social.Tests.Integration.Seeds;

internal static class FollowRequestSeed
{
    public static readonly Guid FormerlyFollowedUser = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
    public static readonly Guid RejectingUser = Guid.Parse("00000000-0000-0000-0000-0000000000f2");
    public static readonly Guid FollowedUser = Guid.Parse("00000000-0000-0000-0000-0000000000f3");
    public static readonly Guid PendingFollower = Guid.Parse("00000000-0000-0000-0000-0000000000f4");
    public static readonly Guid NotFollowedFollower = Guid.Parse("00000000-0000-0000-0000-0000000000f5");
    public static readonly Guid RequestingUser = Guid.Parse("00000000-0000-0000-0000-0000000000f6");
    public static readonly Guid UnfollowedUser = Guid.Parse("00000000-0000-0000-0000-0000000000f7");

    public static readonly FollowRequest ExplorerToSecondExplorerPending;
    public static readonly FollowRequest SecondExplorerToAdministratorPending;
    public static readonly FollowRequest AdministratorRemovedByExplorer;
    public static readonly FollowRequest ExplorerRejectedByFormerlyFollowedUser;
    public static readonly FollowRequest ExplorerRemovedByFormerlyFollowedUser;
    public static readonly FollowRequest ExplorerRejectedByRejectingUser;
    public static readonly FollowRequest ExplorerAcceptedByFollowedUser;
    public static readonly FollowRequest FollowedUserAcceptedByExplorer;
    public static readonly FollowRequest PendingFollowerAcceptedByExplorer;
    public static readonly FollowRequest ExplorerToPendingFollowerPending;
    public static readonly FollowRequest NotFollowedFollowerAcceptedByExplorer;
    public static readonly FollowRequest RequestingUserToExplorerPending;
    public static readonly FollowRequest FormerlyFollowedUserAcceptedByExplorer;
    public static readonly FollowRequest ExplorerRejectedByUnfollowedUser;
    public static readonly FollowRequest ExplorerUnfollowedUnfollowedUser;
    public static readonly FollowRequest UnfollowedUserAcceptedByExplorer;

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

        FollowedUserAcceptedByExplorer = new(FollowedUser, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        FollowedUserAcceptedByExplorer.Accept(WellKnownUsers.Explorer);

        PendingFollowerAcceptedByExplorer = new(PendingFollower, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        PendingFollowerAcceptedByExplorer.Accept(WellKnownUsers.Explorer);

        ExplorerToPendingFollowerPending = new(WellKnownUsers.Explorer, PendingFollower,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));

        NotFollowedFollowerAcceptedByExplorer = new(NotFollowedFollower, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        NotFollowedFollowerAcceptedByExplorer.Accept(WellKnownUsers.Explorer);

        RequestingUserToExplorerPending = new(RequestingUser, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));

        FormerlyFollowedUserAcceptedByExplorer = new(FormerlyFollowedUser, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        FormerlyFollowedUserAcceptedByExplorer.Accept(WellKnownUsers.Explorer);

        ExplorerRejectedByUnfollowedUser = new(WellKnownUsers.Explorer, UnfollowedUser,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        ExplorerRejectedByUnfollowedUser.Reject(UnfollowedUser);

        ExplorerUnfollowedUnfollowedUser = new(WellKnownUsers.Explorer, UnfollowedUser,
            new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc));
        ExplorerUnfollowedUnfollowedUser.Accept(UnfollowedUser);
        ExplorerUnfollowedUnfollowedUser.Unfollow(WellKnownUsers.Explorer);

        UnfollowedUserAcceptedByExplorer = new(UnfollowedUser, WellKnownUsers.Explorer,
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        UnfollowedUserAcceptedByExplorer.Accept(WellKnownUsers.Explorer);
    }

    public static FollowRequest[] All =>
    [
        ExplorerToSecondExplorerPending,
        SecondExplorerToAdministratorPending,
        AdministratorRemovedByExplorer,
        ExplorerRejectedByFormerlyFollowedUser,
        ExplorerRemovedByFormerlyFollowedUser,
        ExplorerRejectedByRejectingUser,
        ExplorerAcceptedByFollowedUser,
        FollowedUserAcceptedByExplorer,
        PendingFollowerAcceptedByExplorer,
        ExplorerToPendingFollowerPending,
        NotFollowedFollowerAcceptedByExplorer,
        RequestingUserToExplorerPending,
        FormerlyFollowedUserAcceptedByExplorer,
        ExplorerRejectedByUnfollowedUser,
        ExplorerUnfollowedUnfollowedUser,
        UnfollowedUserAcceptedByExplorer
    ];
}
