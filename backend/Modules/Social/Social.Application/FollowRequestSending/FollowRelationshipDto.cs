using Social.Application.FollowRequests;

namespace Social.Application.FollowRequestSending;

public sealed record FollowRelationshipDto(FollowRelationshipState State, Guid? PendingRequestId);
