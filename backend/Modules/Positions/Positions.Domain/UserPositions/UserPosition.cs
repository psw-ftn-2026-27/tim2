using Shared.Domain;

namespace Positions.Domain.UserPositions;

public sealed class UserPosition : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Coordinates Coordinates { get; private set; } = null!;

    private UserPosition() { }

    public UserPosition(Guid userId, Coordinates coordinates) : base(Guid.NewGuid())
    {
        UserId = userId;
        Coordinates = coordinates;
    }

    public void MoveTo(Coordinates coordinates)
    {
        Coordinates = coordinates;
    }
}
