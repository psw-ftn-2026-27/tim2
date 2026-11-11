using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletFunding;
using Payment.Application.Wallets;
using Payment.Application.WalletViewing;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int pageSize)
    {
        var transactions = _dbContext.Wallets
            .Where(wallet => wallet.UserId == userId)
            .SelectMany(wallet => wallet.Transactions)
            .OrderByDescending(transaction => transaction.CreatedAt);

        var totalCount = await transactions.CountAsync();
        var items = await transactions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(transaction => new TransactionDto(
                transaction.Id,
                transaction.Amount,
                transaction.CreatedAt,
                transaction.IsCancelled))
            .ToListAsync();

        return new PageResult<TransactionDto>(items, totalCount);
    }

    public Task<List<UserBalanceDto>> GetBalancesAsync(List<Guid> userIds)
    {
        return _dbContext.Wallets
            .Where(wallet => userIds.Contains(wallet.UserId))
            .Select(wallet => new UserBalanceDto(
                wallet.UserId,
                wallet.Transactions
                    .Where(transaction => !transaction.IsCancelled)
                    .Sum(transaction => transaction.Amount)))
            .ToListAsync();
    }
}
