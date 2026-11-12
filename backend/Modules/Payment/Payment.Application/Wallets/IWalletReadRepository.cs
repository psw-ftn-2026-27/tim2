using Payment.Application.WalletFunding;
using Payment.Application.WalletViewing;
using Shared.Domain;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<PageResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int pageSize);

    Task<List<UserBalanceDto>> GetBalancesAsync(List<Guid> userIds);

    Task<PageResult<UserTransactionDto>> GetUserTransactionsAsync(Guid userId, int page, int pageSize);
}
