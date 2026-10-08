using MultitransferVaslAdapter.Common;
using MultitransferVaslAdapter.Dtos.Vasl;
using MultitransferVaslAdapter.Options;

namespace MultitransferVaslAdapter.Vasl;

public sealed class VaslGateway : IVaslGateway
{
    private readonly IHttpClientFactory _factory;
    private readonly VaslOptions _opt;

    public VaslGateway(IHttpClientFactory factory, IOptions<VaslOptions> opt)
    {
        _factory = factory;
        _opt = opt.Value;
    }

    public Task<VaslBaseResponse> CheckAccountAsync(VaslCheckAccountRequest req, string requestId, CancellationToken ct)
    {
        req.PartnerId = _opt.PartnerId;
        req.ServiceId = _opt.ServiceId;
        req.Key = Md5.HexLower(_opt.SecretKey + req.PartnerId);
        return Post<VaslBaseResponse>($"{_opt.BaseUrl.TrimEnd('/')}/checkaccount", req, requestId, ct);
    }

    public Task<VaslBaseResponse> PayAsync(VaslPayRequest req, string requestId, CancellationToken ct)
    {
        req.PartnerId = _opt.PartnerId;
        req.ServiceId = _opt.ServiceId;
        req.Key = Md5.HexLower(_opt.SecretKey + req.PartnerId + req.Id);
        return Post<VaslBaseResponse>($"{_opt.BaseUrl.TrimEnd('/')}/pay", req, requestId, ct);
    }

    public Task<VaslStatusResponse> GetStatusAsync(VaslStatusRequest req, string requestId, CancellationToken ct)
    {
        req.PartnerId = _opt.PartnerId;
        req.ServiceId = _opt.ServiceId;
        req.Key = Md5.HexLower(_opt.SecretKey + req.PartnerId);
        return Post<VaslStatusResponse>($"{_opt.BaseUrl.TrimEnd('/')}/gettransactionstatus", req, requestId, ct);
    }

    public Task<VaslBalanceResponse> GetBalanceAsync(VaslBalanceRequest req, string requestId, CancellationToken ct)
    {
        req.PartnerId = _opt.PartnerId;
        req.Key = Md5.HexLower(_opt.SecretKey + req.PartnerId);
        return Post<VaslBalanceResponse>($"{_opt.BaseUrl.TrimEnd('/')}/getbalance", req, requestId, ct);
    }

    private async Task<T> Post<T>(string url, object body, string requestId, CancellationToken ct)
    {
        var http = _factory.CreateClient("vasl");
        http.Timeout = TimeSpan.FromSeconds(_opt.TimeoutSeconds);

        using var msg = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        msg.Headers.TryAddWithoutValidation("x-request-id", requestId);

        using var res = await http.SendAsync(msg, ct);
        res.EnsureSuccessStatusCode();

        var parsed = await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        if (parsed is null) throw new InvalidOperationException("Empty Vasl response");
        return parsed;
    }
}