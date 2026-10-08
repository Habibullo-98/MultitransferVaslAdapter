namespace MultitransferVaslAdapter.Controllers;

[EnableRateLimiting("partner")]
[ApiController]
[Route("transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _svc;
    public TransactionsController(ITransactionService svc) => _svc = svc;

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequestDto req, CancellationToken ct)
    {
        var rid = HttpContext.Request.Headers["x-request-id"].ToString();
        try { return Ok(await _svc.CreateAsync(req, rid, ct)); }
        catch (Exception ex) { return BadRequest(new { error = ErrorFactory.Fail(101, "Incorrect request", details: ex.Message) }); }
    }

    [HttpPost("{transactionId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid transactionId, CancellationToken ct)
    {
        var rid = HttpContext.Request.Headers["x-request-id"].ToString();
        return Ok(await _svc.ConfirmAsync(transactionId, rid, ct));
    }

    [HttpGet("{transactionId:guid}/status")]
    public async Task<IActionResult> Status(Guid transactionId, CancellationToken ct)
    {
        var rid = HttpContext.Request.Headers["x-request-id"].ToString();
        return Ok(await _svc.StatusAsync(transactionId, rid, ct));
    }

    [HttpGet("registry")]
    public async Task<IActionResult> Registry([FromQuery] DateOnly date, CancellationToken ct)
        => Ok(await _svc.RegistryAsync(date, ct));

    [HttpPatch("{transactionId:guid}")]
    public async Task<IActionResult> Patch(Guid transactionId, [FromBody] PatchBeneficiaryRequestDto req, CancellationToken ct)
        => Ok(await _svc.PatchBeneficiaryAsync(transactionId, req, ct));

    [HttpPost("{transactionId:guid}/refund")]
    public async Task<IActionResult> Refund(Guid transactionId, [FromBody] RefundRequestDto req, CancellationToken ct)
        => Ok(await _svc.RefundAsync(transactionId, req, ct));
}