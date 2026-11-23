using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletViewing;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize(Roles = "explorer")]
public sealed class WalletViewingController : ControllerBase
{
    private readonly WalletViewingQueries _viewingQueries;

    public WalletViewingController(WalletViewingQueries viewingQueries)
    {
        _viewingQueries = viewingQueries;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance()
    {
        return await _viewingQueries.GetBalanceAsync(User.GetUserId());
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<PageResult<TransactionDto>>> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _viewingQueries.GetTransactionsAsync(User.GetUserId(), page, pageSize);
    }
}
