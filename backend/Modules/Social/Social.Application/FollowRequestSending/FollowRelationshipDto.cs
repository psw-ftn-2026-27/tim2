namespace Social.Application.FollowRequestSending;

public sealed record FollowRelationshipDto(FollowRelationshipState State, Guid? PendingRequestId);
