namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<ExplorerPage> GetExplorersAsync(int page, int pageSize);

    Task<ExplorerSummary?> GetExplorerAsync(Guid userId);
}
