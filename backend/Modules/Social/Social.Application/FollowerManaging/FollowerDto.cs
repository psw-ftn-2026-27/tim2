using Social.Application.FollowRequests;

namespace Social.Application.FollowerManaging;

public sealed record FollowerDto(Guid UserId, FollowRelationshipState State, Guid? PendingRequestId);
