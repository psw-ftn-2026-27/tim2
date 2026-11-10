using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public int Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid AdminId { get; private set; }
    public bool IsCancelled { get; private set; }

    private Transaction() { }

    public Transaction(Guid adminId, int amount) : base(Guid.NewGuid())
    {
        AdminId = adminId;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        IsCancelled = false;
    }
}
