using Identity.Contracts;
using Payment.Application.Wallets;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletFunding;

public sealed class WalletFundingQueries
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IIdentityApi _identityApi;

    public WalletFundingQueries(IWalletRepository walletRepository, IWalletReadRepository walletReadRepository, IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _walletReadRepository = walletReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<UserWalletDto>> GetWalletsAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var explorers = await _identityApi.GetExplorersAsync(page, pageSize);
        var balances = await _walletReadRepository.GetBalancesAsync(
            explorers.Items.Select(explorer => explorer.UserId).ToList());

        var items = explorers.Items
            .Select(explorer => new UserWalletDto(
                explorer.UserId,
                explorer.Email,
                balances
                    .Where(balance => balance.UserId == explorer.UserId)
                    .Sum(balance => balance.Balance)))
            .ToList();

        return new PageResult<UserWalletDto>(items, explorers.TotalCount);
    }

    public async Task<UserWalletDto> GetWalletAsync(Guid userId)
    {
        var explorer = await _identityApi.GetExplorerAsync(userId);
        if (explorer is null)
        {
            throw new NotFoundException($"User {userId} does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        return new UserWalletDto(explorer.UserId, explorer.Email, wallet?.Balance() ?? 0);
    }

    public Task<PageResult<UserTransactionDto>> GetWalletTransactionsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _walletReadRepository.GetUserTransactionsAsync(userId, page, pageSize);
    }
}
