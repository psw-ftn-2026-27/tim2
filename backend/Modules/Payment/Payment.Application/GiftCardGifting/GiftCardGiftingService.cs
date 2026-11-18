using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCardGifting;

public sealed class GiftCardGiftingService
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public GiftCardGiftingService(
        IGiftCardRepository giftCardRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        IIdentityApi identityApi)
    {
        _giftCardRepository = giftCardRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task<Guid> PurchaseAsync(Guid senderId, PurchaseGiftCardDto dto)
    {
        var recipient = await _identityApi.GetExplorerByEmailAsync(dto.RecipientEmail);
        if (recipient is null)
        {
            throw new NotFoundException($"User {dto.RecipientEmail} does not exist.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(senderId);
        if (wallet is null)
        {
            wallet = new Wallet(senderId);
            _walletRepository.Add(wallet);
        }

        var giftCard = new GiftCard(senderId, recipient.UserId, dto.Amount, dto.Message, DateTime.UtcNow);
        wallet.PayGiftCard(giftCard.Id, giftCard.Amount);
        _giftCardRepository.Add(giftCard);
        await _unitOfWork.SaveChangesAsync();

        return giftCard.Id;
    }
}
