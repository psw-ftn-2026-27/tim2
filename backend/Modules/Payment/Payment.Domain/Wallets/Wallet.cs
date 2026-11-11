using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    public Guid UserId { get; private set; }
    private readonly List<Transaction> _transactions = [];
    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public int Balance()
    {
        return _transactions
            .Where(transaction => !transaction.IsCancelled)
            .Sum(transaction => transaction.Amount);
    }

    public void Deposit(Guid adminId, int amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Deposit amount must be a positive whole number.");
        }

        _transactions.Add(new Transaction(adminId, amount));
    }

    public void CancelDeposit(Guid transactionId, Guid adminId, DateTime now)
    {
        var transaction = _transactions.FirstOrDefault(transaction => transaction.Id == transactionId);
        if (transaction is null)
        {
            throw new NotFoundException($"Transaction {transactionId} does not exist.");
        }

        if (Balance() - transaction.Amount < 0)
        {
            throw new DomainException("Cancelling the deposit would make the wallet balance negative.");
        }

        transaction.Cancel(adminId, now);
    }
}
