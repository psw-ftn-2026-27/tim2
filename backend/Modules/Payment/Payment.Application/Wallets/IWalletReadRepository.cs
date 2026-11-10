using Payment.Application.WalletViewing;
using Shared.Domain;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<PageResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int pageSize);
}
