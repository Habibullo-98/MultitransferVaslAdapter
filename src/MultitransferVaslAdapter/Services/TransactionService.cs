namespace MultitransferVaslAdapter.Services;

public interface ITransactionService
{
    Task<CreateTransactionResponseDto> CreateAsync(CreateTransactionRequestDto req, string requestId, CancellationToken ct);
    Task<CreateTransactionResponseDto> ConfirmAsync(Guid transactionId, string requestId, CancellationToken ct);
    Task<StatusResponseDto> StatusAsync(Guid transactionId, string requestId, CancellationToken ct);
    Task<RegistryResponseDto> RegistryAsync(DateOnly date, CancellationToken ct);
    Task<PatchBeneficiaryResponseDto> PatchBeneficiaryAsync(Guid transactionId, PatchBeneficiaryRequestDto req, CancellationToken ct);
    Task<RefundResponseDto> RefundAsync(Guid transactionId, RefundRequestDto req, CancellationToken ct);
    Task<AccountBalanceResponseDto> AccountBalanceAsync(string requestId, string account, CancellationToken ct);
}

public sealed class TransactionService : ITransactionService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly AdapterDbContext _db;
    private readonly IVaslGateway _vasl;
    private readonly IIdempotencyStore _idem;
    private readonly ILogger<TransactionService> _log;

    public TransactionService(AdapterDbContext db, IVaslGateway vasl, IIdempotencyStore idem, ILogger<TransactionService> log)
    {
        _db = db;
        _vasl = vasl;
        _idem = idem;
        _log = log;
    }

    public async Task<CreateTransactionResponseDto> CreateAsync(CreateTransactionRequestDto req, string requestId, CancellationToken ct)
    {
        ValidateCreate(req);

        var idemKey = $"mt:create:{req.TransactionId}";
        var cached = await _idem.GetAsync(idemKey, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<CreateTransactionResponseDto>(cached, JsonOpts)!;

        var callsig = ResolveCallsign(req);
        var now = DateTime.UtcNow;

        var entity = await _db.Transactions.FindAsync([req.TransactionId], ct);
        if (entity is not null)
        {
            var respExisting = ToCreateResponse(req, entity.Status, entity.ExternalTransactionNum, entity.UpdatedAtUtc, ErrorFactory.Success());
            await Cache(idemKey, respExisting, ct);
            return respExisting;
        }

        entity = new TransactionEntity
        {
            TransactionId = req.TransactionId,
            RecordType = req.RecordType,
            ServiceId = req.ServiceId,
            CountryCode = req.CountryCode,
            Status = "CREATED",
            ExternalTransactionNum = req.TransactionId.ToString("N"),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            BeneficiaryAccountNumber = req.Transaction?.BeneficiaryAccountNumber,
            BeneficiaryPhoneNumber = req.Transaction?.BeneficiaryPhoneNumber ?? req.Beneficiary?.PhoneNumber,
            SenderComment = req.Transaction?.SenderComment,
            RawRequestJson = JsonSerializer.Serialize(req, JsonOpts),
            LastErrorCode = 0
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(ct);

        // 1) CheckAccount
        var checkReq = new VaslCheckAccountRequest
        {
            Id = req.TransactionId.ToString(),
            Amount = req.Money.WithdrawMoney.Amount,
            CurrencyValue = req.Money.Rate,
            CallsigNumber = callsig,
            Params = BuildVaslParams(req)
        };

        var check = await _vasl.CheckAccountAsync(checkReq, requestId, ct);
        if (check.Result != 1)
        {
            var fail = ToCreateResponse(req, "REJECTED", entity.ExternalTransactionNum, DateTime.UtcNow,
                ErrorFactory.Fail(110, "Operation can not be processed", check.Comment));

            await SaveFail(entity, fail.Error, check.Comment, "REJECTED", ct);
            await Cache(idemKey, fail, ct);
            return fail;
        }

        entity.Status = "CREATED";
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.LastErrorCode = 0;
        entity.LastErrorExternal = null;
        await _db.SaveChangesAsync(ct);

        var ok = ToCreateResponse(req, "CREATED", entity.ExternalTransactionNum, entity.UpdatedAtUtc, ErrorFactory.Success());
        await Cache(idemKey, ok, ct);
        return ok;
    }

    public async Task<CreateTransactionResponseDto> ConfirmAsync(Guid transactionId, string requestId, CancellationToken ct)
    {
        var idemKey = $"mt:confirm:{transactionId}";
        var cached = await _idem.GetAsync(idemKey, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<CreateTransactionResponseDto>(cached, JsonOpts)!;

        var entity = await _db.Transactions.FindAsync([transactionId], ct);
        if (entity is null)
            return new CreateTransactionResponseDto { TransactionId = transactionId, Error = ErrorFactory.Fail(108, "Entity not found") };

        var req = JsonSerializer.Deserialize<CreateTransactionRequestDto>(entity.RawRequestJson, JsonOpts)!;
        var callsig = ResolveCallsign(req);

        var stReq = new VaslStatusRequest
        {
            Id = transactionId.ToString(),
            Amount = req.Money.WithdrawMoney.Amount,
            CurrencyValue = req.Money.Rate,
            CallsigNumber = callsig
        };

        // 2) Pay
        var payReq = new VaslPayRequest
        {
            Id = req.TransactionId.ToString(),
            Amount = req.Money.WithdrawMoney.Amount,
            CurrencyValue = req.Money.Rate,
            CallsigNumber = callsig,
            Params = BuildVaslParams(req)
        };

        var pay = await _vasl.PayAsync(payReq, requestId, ct);
        if (pay.Result != 1)
        {
            var fail = ToCreateResponse(req, "REJECTED", entity.ExternalTransactionNum, DateTime.UtcNow,
                ErrorFactory.Fail(110, "Payment rejected", pay.Comment));

            await SaveFail(entity, fail.Error, pay.Comment, "REJECTED", ct);
            await Cache(idemKey, fail, ct);
            return fail;
        }

        var mapped = MapStatus(1);

        entity.Status = "";
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var resp = ToCreateResponse(req, mapped, entity.ExternalTransactionNum, entity.UpdatedAtUtc,
            pay.Result == 1 ? ErrorFactory.Success() : ErrorFactory.Fail(102, "Invalid backend response", pay.Comment));

        await Cache(idemKey, resp, ct);
        return resp;
    }

    public async Task<StatusResponseDto> StatusAsync(Guid transactionId, string requestId, CancellationToken ct)
    {
        var entity = await _db.Transactions.FindAsync([transactionId], ct);
        if (entity is null)
            return new StatusResponseDto { TransactionId = transactionId, TransactionState = "REJECTED", Error = ErrorFactory.Fail(108, "Entity not found") };

        var req = JsonSerializer.Deserialize<CreateTransactionRequestDto>(entity.RawRequestJson, JsonOpts)!;
        var callsig = ResolveCallsign(req);

        var stReq = new VaslStatusRequest
        {
            Id = transactionId.ToString(),
            Amount = req.Money.WithdrawMoney.Amount,
            CurrencyValue = req.Money.Rate,
            CallsigNumber = callsig
        };

        var st = await _vasl.GetStatusAsync(stReq, requestId, ct);
        var mapped = MapStatus(st.Status);

        entity.Status = mapped;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new StatusResponseDto
        {
            TransactionId = transactionId,
            TransactionState = mapped,
            Reason = st.Comment,
            PaymentDateUpdated = entity.UpdatedAtUtc,
            Error = ErrorFactory.Success()
        };
    }

    public async Task<RegistryResponseDto> RegistryAsync(DateOnly date, CancellationToken ct)
    {
        var from = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var to = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var rows = await _db.Transactions
            .Where(x => x.CreatedAtUtc >= from && x.CreatedAtUtc < to)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        var resp = new RegistryResponseDto();
        foreach (var e in rows)
        {
            var req = JsonSerializer.Deserialize<CreateTransactionRequestDto>(e.RawRequestJson, JsonOpts)!;
            resp.Transactions.Add(ToCreateResponse(req, e.Status, e.ExternalTransactionNum, e.UpdatedAtUtc, ErrorFactory.Success()));
        }
        return resp;
    }

    public async Task<PatchBeneficiaryResponseDto> PatchBeneficiaryAsync(Guid transactionId, PatchBeneficiaryRequestDto patch, CancellationToken ct)
    {
        var entity = await _db.Transactions.FindAsync([transactionId], ct);
        if (entity is null)
            return new PatchBeneficiaryResponseDto { TransactionId = transactionId, TransactionState = "REJECTED", Error = ErrorFactory.Fail(108, "Entity not found") };

        var req = JsonSerializer.Deserialize<CreateTransactionRequestDto>(entity.RawRequestJson, JsonOpts)!;
        req.Beneficiary ??= new BeneficiaryDto { FirstName = "", LastName = "" };

        if (patch.BeneficiaryFirstName is not null) req.Beneficiary.FirstName = patch.BeneficiaryFirstName;
        if (patch.BeneficiaryLastName is not null) req.Beneficiary.LastName = patch.BeneficiaryLastName;
        if (patch.BeneficiaryMiddleName is not null) req.Beneficiary.MiddleName = patch.BeneficiaryMiddleName;
        if (patch.BirthDate is not null) req.Beneficiary.BirthDate = patch.BirthDate;

        entity.RawRequestJson = JsonSerializer.Serialize(req, JsonOpts);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new PatchBeneficiaryResponseDto
        {
            TransactionId = transactionId,
            TransactionState = entity.Status,
            BeneficiaryFirstName = req.Beneficiary.FirstName,
            BeneficiaryLastName = req.Beneficiary.LastName,
            BeneficiaryMiddleName = req.Beneficiary.MiddleName,
            BirthDate = req.Beneficiary.BirthDate,
            Error = ErrorFactory.Success()
        };
    }

    public async Task<RefundResponseDto> RefundAsync(Guid transactionId, RefundRequestDto req, CancellationToken ct)
    {
        var entity = await _db.Transactions.FindAsync([transactionId], ct);
        if (entity is null)
            return new RefundResponseDto { TransactionId = transactionId, TransactionState = "REJECTED", Error = ErrorFactory.Fail(108, "Entity not found") };

        entity.Status = "TO_REFUND";
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new RefundResponseDto
        {
            TransactionId = transactionId,
            TransactionState = "TO_REFUND",
            Error = ErrorFactory.Success()
        };
    }

    public async Task<AccountBalanceResponseDto> AccountBalanceAsync(string requestId, string account, CancellationToken ct)
    {
        var bal = await _vasl.GetBalanceAsync(new VaslBalanceRequest(), requestId, ct);
        return new AccountBalanceResponseDto
        {
            Accounts = new List<AccountBalanceItemDto>
            {
                new AccountBalanceItemDto
                {
                    Account = account,          //Name
                    Currency = account,         //CCY
                    Debit = bal.Balance,        // Balance From TX 
                    Credit = bal.Overdraft      // Credit From TX Usually 0
                }
            },
            Error = ErrorFactory.Success()
        };
    }

    private static void ValidateCreate(CreateTransactionRequestDto req)
    {
        if (req.TransactionId == Guid.Empty) throw new ValidationException("transactionId is required");
        if (string.IsNullOrWhiteSpace(req.RecordType)) throw new ValidationException("recordType is required");
        if (req.Money?.WithdrawMoney is null) throw new ValidationException("money.withdrawMoney is required");
        if (req.Money.WithdrawMoney.Amount <= 0) throw new ValidationException("withdrawMoney.amount must be > 0");
        if (string.IsNullOrWhiteSpace(req.Money.WithdrawMoney.CurrencyCode)) throw new ValidationException("withdrawMoney.currencyCode is required");
    }

    private static string ResolveCallsign(CreateTransactionRequestDto req)
    {
        var callsig = req.Transaction?.BeneficiaryAccountNumber
                     ?? req.Transaction?.BeneficiaryPhoneNumber
                     ?? req.Beneficiary?.PhoneNumber;

        if (string.IsNullOrWhiteSpace(callsig))
            throw new ValidationException("beneficiaryAccountNumber or beneficiaryPhoneNumber is required");

        return callsig;
    }

    private static string MapStatus(int vaslStatus) => vaslStatus switch
    {
        1 => "PROCESSING",
        2 => "PROCESSED",
        3 => "REJECTED",
        _ => "PROCESSING"
    };

    private static CreateTransactionResponseDto ToCreateResponse(CreateTransactionRequestDto req, string status, string externalNum, DateTime dateUtc, ErrorDto error)
        => new()
        {
            TransactionId = req.TransactionId,
            RecordType = req.RecordType,
            ServiceId = req.ServiceId,
            ExternalTransactionNum = externalNum,
            TransactionStatus = status,
            TransactionDate = dateUtc,
            CountryCode = req.CountryCode,
            BeneficiaryAccountNumber = req.Transaction?.BeneficiaryAccountNumber,
            BeneficiaryPhoneNumber = req.Transaction?.BeneficiaryPhoneNumber ?? req.Beneficiary?.PhoneNumber,
            SenderComment = req.Transaction?.SenderComment,
            Beneficiary = req.Beneficiary,
            Money = req.Money,
            Error = error
        };

    private static List<VaslParam>? BuildVaslParams(CreateTransactionRequestDto mt)
    {
        var list = new List<VaslParam>();

        if (!string.IsNullOrWhiteSpace(mt.Sender?.PhoneNumber))
            list.Add(new VaslParam { Comment = "PhoneNumber", Values = mt.Sender.PhoneNumber! });

        if (mt.Sender is not null)
        {
            var fio = $"{mt.Sender.LastName} {mt.Sender.FirstName} {mt.Sender.MiddleName}".Trim();
            if (!string.IsNullOrWhiteSpace(fio))
                list.Add(new VaslParam { Comment = "FIO", Values = fio });
        }

        return list.Count == 0 ? null : list;
    }

    private async Task SaveFail(TransactionEntity entity, ErrorDto err, string? external, string status, CancellationToken ct)
    {
        entity.Status = status;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.LastErrorCode = err.Code;
        entity.LastErrorExternal = external;
        await _db.SaveChangesAsync(ct);
    }

    private async Task Cache(string key, CreateTransactionResponseDto resp, CancellationToken ct)
        => await _idem.SetAsync(key, JsonSerializer.Serialize(resp, JsonOpts), TimeSpan.FromDays(7), ct);

    private sealed class ValidationException : Exception { public ValidationException(string m) : base(m) { } }
}