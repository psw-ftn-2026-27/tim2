using Payment.Application.Wallets;
using Shared.Domain;

namespace Payment.Application.WalletViewing;

public sealed class WalletViewingQueries
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletReadRepository _walletReadRepository;

    public WalletViewingQueries(IWalletRepository walletRepository, IWalletReadRepository walletReadRepository)
    {
        _walletRepository = walletRepository;
        _walletReadRepository = walletReadRepository;
    }

    public async Task<WalletBalanceDto> GetBalanceAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);

        return new WalletBalanceDto(wallet?.Balance() ?? 0);
    }

    public Task<PageResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _walletReadRepository.GetTransactionsAsync(userId, page, pageSize);
    }
}
