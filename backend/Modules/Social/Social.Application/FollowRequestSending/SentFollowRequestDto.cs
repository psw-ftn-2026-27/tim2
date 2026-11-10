using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequestSending;

public sealed record SentFollowRequestDto(Guid Id, Guid ReceiverId, FollowRequestStatus Status);
