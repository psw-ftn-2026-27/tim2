using Identity.Contracts;
using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Application.WalletFunding;

public sealed class WalletFundingQueries
{
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IIdentityApi _identityApi;

    public WalletFundingQueries(IWalletReadRepository walletReadRepository, IIdentityApi identityApi)
    {
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
}
