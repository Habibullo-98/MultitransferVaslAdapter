namespace MultitransferVaslAdapter.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        if (!ctx.Request.Headers.TryGetValue("x-request-id", out var rid) || string.IsNullOrWhiteSpace(rid))
            ctx.Request.Headers["x-request-id"] = Guid.NewGuid().ToString();

        ctx.Response.Headers["x-request-id"] = ctx.Request.Headers["x-request-id"].ToString();
        await _next(ctx);
    }
}
