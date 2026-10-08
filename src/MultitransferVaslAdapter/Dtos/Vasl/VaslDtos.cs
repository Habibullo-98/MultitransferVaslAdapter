namespace MultitransferVaslAdapter.Dtos.Vasl;

public sealed class VaslParam
{
    [JsonPropertyName("comment")] public string Comment { get; set; } = default!;
    [JsonPropertyName("values")]  public string Values  { get; set; } = default!;
}

public sealed class VaslCheckAccountRequest
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("currencyvalue")] public decimal? CurrencyValue { get; set; }
    [JsonPropertyName("serviceId")] public int ServiceId { get; set; }
    [JsonPropertyName("callsigNumber")] public string CallsigNumber { get; set; } = default!;
    [JsonPropertyName("partnerId")] public int PartnerId { get; set; }
    [JsonPropertyName("key")] public string Key { get; set; } = default!;
    [JsonPropertyName("params")] public List<VaslParam>? Params { get; set; }
}

public sealed class VaslPayRequest
{
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("currencyvalue")] public decimal? CurrencyValue { get; set; }
    [JsonPropertyName("serviceId")] public int ServiceId { get; set; }
    [JsonPropertyName("callsigNumber")] public string CallsigNumber { get; set; } = default!;
    [JsonPropertyName("partnerId")] public int PartnerId { get; set; }
    [JsonPropertyName("key")] public string Key { get; set; } = default!;
    [JsonPropertyName("params")] public List<VaslParam>? Params { get; set; }
}

public sealed class VaslStatusRequest
{
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("currencyvalue")] public decimal? CurrencyValue { get; set; }
    [JsonPropertyName("serviceId")] public int ServiceId { get; set; }
    [JsonPropertyName("callsigNumber")] public string CallsigNumber { get; set; } = default!;
    [JsonPropertyName("partnerId")] public int PartnerId { get; set; }
    [JsonPropertyName("key")] public string Key { get; set; } = default!;
}

public sealed class VaslBalanceRequest
{
    [JsonPropertyName("partnerId")] public int PartnerId { get; set; }
    [JsonPropertyName("key")] public string Key { get; set; } = default!;
}

public class VaslBaseResponse
{
    [JsonPropertyName("Result")] public int Result { get; set; }
    [JsonPropertyName("Comment")] public string? Comment { get; set; }
}

public sealed class VaslStatusResponse : VaslBaseResponse
{
    [JsonPropertyName("Status")] public int Status { get; set; }
}

public sealed class VaslBalanceItem
{
    [JsonPropertyName("account")] public string Account { get; set; } = default!;
    [JsonPropertyName("currency")] public string Currency { get; set; } = default!;
    [JsonPropertyName("debit")] public decimal Debit { get; set; }
    [JsonPropertyName("credit")] public decimal Credit { get; set; }
}

public sealed class VaslBalanceResponse
{
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("overdraft")] public decimal Overdraft { get; set; }
}