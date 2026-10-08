namespace MultitransferVaslAdapter.Vasl;

public interface IVaslGateway
{
    Task<VaslBaseResponse> CheckAccountAsync(VaslCheckAccountRequest req, string requestId, CancellationToken ct);
    Task<VaslBaseResponse> PayAsync(VaslPayRequest req, string requestId, CancellationToken ct);
    Task<VaslStatusResponse> GetStatusAsync(VaslStatusRequest req, string requestId, CancellationToken ct);
    Task<VaslBalanceResponse> GetBalanceAsync(VaslBalanceRequest req, string requestId, CancellationToken ct);
}
