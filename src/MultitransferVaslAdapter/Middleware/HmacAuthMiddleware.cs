namespace MultitransferVaslAdapter.Middleware;

public sealed class HmacAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HmacOptions _opt;

    public HmacAuthMiddleware(RequestDelegate next, IOptions<HmacOptions> opt)
    {
        _next = next;
        _opt = opt.Value;
    }

    public async Task Invoke(HttpContext ctx)
    {
        var p = ctx.Request.Path.Value ?? "";
        if (p.StartsWith("/health") || p.StartsWith("/metrics") || p.StartsWith("/swagger"))
        {
            await _next(ctx);
            return;
        }

        if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var apiKey) ||
            !ctx.Request.Headers.TryGetValue("X-Signature", out var sig) ||
            !ctx.Request.Headers.TryGetValue("X-Timestamp", out var ts))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!string.Equals(apiKey.ToString(), _opt.ApiKey, StringComparison.Ordinal))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!long.TryParse(ts.ToString(), out var unix))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - unix) > _opt.AllowedSkewSeconds)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        ctx.Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            ctx.Request.Body.Position = 0;
        }

        var bodyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body ?? string.Empty))).ToLowerInvariant();
        var path = Uri.UnescapeDataString(ctx.Request.Path.ToString());
        var canonical = $"{ctx.Request.Method}\n{path}\n{unix}\n{bodyHash}";
        var expected = ComputeHmac(canonical, _opt.Secret);

        if (!FixedTimeEqualsHex(expected, sig.ToString()))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(ctx);
    }

    private static string ComputeHmac(string data, string secret)
    {
        var sec = Convert.ToHexString(Encoding.UTF8.GetBytes(secret));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private static bool FixedTimeEqualsHex(string a, string b)
    {
        var aa = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return aa.Length == bb.Length && CryptographicOperations.FixedTimeEquals(aa, bb);
    }
}