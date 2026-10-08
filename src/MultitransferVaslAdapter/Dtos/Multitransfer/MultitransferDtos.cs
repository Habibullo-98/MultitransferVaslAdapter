namespace MultitransferVaslAdapter.Dtos.Multitransfer;

public sealed class ErrorDto
{
    public int Code { get; set; }
    public string? Message { get; set; }
    public string? ExternalCode { get; set; }
    public string? Details { get; set; }
    public Guid Uuid { get; set; }
}

public static class ErrorFactory
{
    public static ErrorDto Success() => new() { Code = 0, Uuid = Guid.NewGuid() };
    public static ErrorDto Fail(int code, string message, string? external = null, string? details = null)
        => new() { Code = code, Message = message, ExternalCode = external, Details = details, Uuid = Guid.NewGuid() };
}

public sealed class AmountDto { public decimal Amount { get; set; } public string CurrencyCode { get; set; } = default!; }
public sealed class FeeDto { public decimal Amount { get; set; } public string CurrencyCode { get; set; } = default!; }

public sealed class MoneyDto
{
    public AmountDto SettlementMoney { get; set; } = default!;
    public AmountDto WithdrawMoney { get; set; } = default!;
    public FeeDto? Fee { get; set; }
    public decimal Rate { get; set; }
}

public sealed class SenderDto
{
    public string LastName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
}

public sealed class BeneficiaryDto
{
    public string LastName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
}

public sealed class TransactionMetaDto
{
    public string? SenderComment { get; set; }
    public string? BeneficiaryAccountNumber { get; set; }
    public string? BeneficiaryPhoneNumber { get; set; }
}

public sealed class CreateTransactionRequestDto
{
    public Guid TransactionId { get; set; }
    public string RecordType { get; set; } = default!;
    public Guid? ServiceId { get; set; }
    public string CountryCode { get; set; } = default!;
    public SenderDto? Sender { get; set; }
    public BeneficiaryDto? Beneficiary { get; set; }
    public MoneyDto Money { get; set; } = default!;
    public TransactionMetaDto Transaction { get; set; } = default!;
}

public class RegistryTransactionResponseDto
{
    public Guid TransactionId { get; set; }
    public string RecordType { get; set; } = default!;
    public Guid? ServiceId { get; set; }
    public string ExternalTransactionNum { get; set; } = default!;
    public string TransactionStatus { get; set; } = default!;
    public DateTime TransactionDate { get; set; }
    public string CountryCode { get; set; } = default!;
    public MoneyDto Money { get; set; } = default!;
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class CreateTransactionResponseDto : RegistryTransactionResponseDto
{
    //public Guid TransactionId { get; set; }
    //public string RecordType { get; set; } = default!;
    //public Guid? ServiceId { get; set; }
    //public string ExternalTransactionNum { get; set; } = default!;
    //public string TransactionStatus { get; set; } = default!;
    //public DateTime TransactionDate { get; set; }
    //public string CountryCode { get; set; } = default!;

    public string? BeneficiaryAccountNumber { get; set; }
    public string? BeneficiaryPhoneNumber { get; set; }
    public string? SenderComment { get; set; }
    public BeneficiaryDto? Beneficiary { get; set; }

    //public MoneyDto Money { get; set; } = default!;
    //public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class StatusResponseDto
{
    public Guid TransactionId { get; set; }
    public string TransactionState { get; set; } = default!;
    public string? Reason { get; set; }
    public DateTime? PaymentDateUpdated { get; set; }
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class RegistryResponseDto
{
    public List<CreateTransactionResponseDto> Transactions { get; set; } = new();
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class RefundRequestDto { public decimal? RefundAmount { get; set; } public string? RefundCurrency { get; set; } public string? RefundComment { get; set; } }

public sealed class RefundResponseDto
{
    public Guid TransactionId { get; set; }
    public string TransactionState { get; set; } = default!;
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class PatchBeneficiaryRequestDto
{
    public string? BeneficiaryFirstName { get; set; }
    public string? BeneficiaryLastName { get; set; }
    public string? BeneficiaryMiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
}

public sealed class PatchBeneficiaryResponseDto
{
    public Guid TransactionId { get; set; }
    public string TransactionState { get; set; } = default!;
    public string? BeneficiaryFirstName { get; set; }
    public string? BeneficiaryLastName { get; set; }
    public string? BeneficiaryMiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class RateResponseDto
{
    public string ServiceId { get; set; } = default!;
    public string CurrencyFrom { get; set; } = default!;
    public string CurrencyTo { get; set; } = default!;
    public decimal? AmountFrom { get; set; }
    public decimal? AmountTo { get; set; }
    public decimal ExchangeRate { get; set; }
    public int? Multy { get; set; }
    public decimal SellRate { get; set; }
    public decimal BuyRate { get; set; }
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}

public sealed class AccountBalanceItemDto { public string Account { get; set; } = default!; public string Currency { get; set; } = default!; public decimal Debit { get; set; } public decimal Credit { get; set; } }

public sealed class AccountBalanceResponseDto
{
    public List<AccountBalanceItemDto> Accounts { get; set; } = new();
    public ErrorDto Error { get; set; } = ErrorFactory.Success();
}
