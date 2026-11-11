using Social.Application.FollowRequests;

namespace Social.Application.Connections;

public sealed record FollowerDto(Guid UserId, FollowRelationshipState State, Guid? PendingRequestId);
