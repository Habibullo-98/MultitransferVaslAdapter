namespace MultitransferVaslAdapter.Middleware;

public sealed class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMappingMiddleware> _log;

    public ExceptionMappingMiddleware(RequestDelegate next, ILogger<ExceptionMappingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (Exception ex)
        {
            var rid = ctx.Request.Headers["x-request-id"].ToString();
            _log.LogError(ex, "Unhandled error rid={rid}", rid);

            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/json; charset=utf-8";

            var err = ErrorFactory.Fail(100, "Unexpected error", details: ex.Message);
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = err }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
    }
}
