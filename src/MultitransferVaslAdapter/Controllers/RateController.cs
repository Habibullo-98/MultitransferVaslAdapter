namespace MultitransferVaslAdapter.Controllers;

[EnableRateLimiting("partner")]
[ApiController]
public sealed class RateController : ControllerBase
{
    private readonly IRateService _rates;
    public RateController(IRateService rates) => _rates = rates;

    [HttpGet("/rate")]
    public async Task<IActionResult> Get([FromQuery] string serviceId, [FromQuery] string currencyFrom, [FromQuery] string currencyTo,
        [FromQuery] decimal amount, [FromQuery] int? multy, CancellationToken ct)
        => Ok(await _rates.GetRateAsync(serviceId, currencyFrom, currencyTo, amount, multy, ct));
}