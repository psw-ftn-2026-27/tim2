namespace Social.Application.FollowRequestResponding;

public sealed record ReceivedFollowRequestDto(Guid Id, Guid SenderId, DateTime CreatedAt);
