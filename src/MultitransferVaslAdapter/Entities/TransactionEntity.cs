namespace MultitransferVaslAdapter.Entities;

public sealed class TransactionEntity
{
    [Key] public Guid TransactionId { get; set; }

    [MaxLength(32)] public string RecordType { get; set; } = default!;
    public Guid? ServiceId { get; set; }

    [MaxLength(8)] public string CountryCode { get; set; } = default!;

    [MaxLength(32)] public string Status { get; set; } = default!;
    [MaxLength(64)] public string ExternalTransactionNum { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    [MaxLength(128)] public string? BeneficiaryAccountNumber { get; set; }
    [MaxLength(32)]  public string? BeneficiaryPhoneNumber { get; set; }
    [MaxLength(256)] public string? SenderComment { get; set; }

    public string RawRequestJson { get; set; } = default!;

    public int LastErrorCode { get; set; }
    [MaxLength(256)] public string? LastErrorExternal { get; set; }
}