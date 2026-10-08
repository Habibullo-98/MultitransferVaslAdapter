namespace MultitransferVaslAdapter.Services;

public interface IRateService
{
    Task<RateResponseDto> GetRateAsync(string serviceId, string from, string to, decimal amount, int? multy, CancellationToken ct);
}

public sealed class RateService : IRateService
{
    private readonly RateOptions _opt;
    public RateService(IOptions<RateOptions> opt) => _opt = opt.Value;

    public Task<RateResponseDto> GetRateAsync(string serviceId, string from, string to, decimal amount, int? multy, CancellationToken ct)
    {
        var key = $"{from.Trim().ToUpperInvariant()}_{to.Trim().ToUpperInvariant()}";
        if (!_opt.Pairs.TryGetValue(key, out var rate))
        {
            return Task.FromResult(new RateResponseDto
            {
                ServiceId = serviceId,
                CurrencyFrom = from,
                CurrencyTo = to,
                Error = ErrorFactory.Fail(102, "Rate is not configured", external: key)
            });
        }

        var amtTo = decimal.Round(amount * rate, 2);
        return Task.FromResult(new RateResponseDto
        {
            ServiceId = serviceId,
            CurrencyFrom = from,
            CurrencyTo = to,
            AmountFrom = amount,
            AmountTo = amtTo,
            ExchangeRate = rate,
            Multy = multy ?? 1,
            SellRate = rate,
            BuyRate = rate,
            Error = ErrorFactory.Success()
        });
    }
}