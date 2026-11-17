namespace Positions.Contracts;

public interface IPositionsApi
{
    Task<PositionSummary?> GetPositionAsync(Guid userId);
}
