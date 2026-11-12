using Shared.Domain;
using Shared.Domain.Exceptions;

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

    public void Cancel(Guid adminId, DateTime now)
    {
        if (adminId != AdminId)
        {
            throw new DomainException("Only the administrator who made the deposit can cancel it.");
        }

        if (IsCancelled)
        {
            throw new DomainException("The deposit is already cancelled.");
        }

        if (now - CreatedAt > TimeSpan.FromHours(24))
        {
            throw new DomainException("A deposit can be cancelled only within 24 hours of being made.");
        }

        IsCancelled = true;
    }
}
