namespace MultitransferVaslAdapter.Controllers;

[EnableRateLimiting("partner")]
[ApiController]
public sealed class BalanceController : ControllerBase
{
    private readonly ITransactionService _svc;
    public BalanceController(ITransactionService svc) => _svc = svc;

    [HttpGet("/accountBalance")]
    public async Task<IActionResult> Get(string account, CancellationToken ct)
    {
        var rid = HttpContext.Request.Headers["x-request-id"].ToString();
        return Ok(await _svc.AccountBalanceAsync(rid, account, ct));
    }
}