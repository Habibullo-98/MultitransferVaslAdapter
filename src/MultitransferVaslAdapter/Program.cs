var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddAdapterServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiTransfer API", Version = "v1" });

    // Фикс для конфликта имен схем
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("partner", cfg =>
    {
        cfg.Window = TimeSpan.FromSeconds(1);
        cfg.PermitLimit = 60;
        cfg.QueueLimit = 0;
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<OpenApiServer>
        {
            new()
            {
                Url = $"https://{httpReq.Host}{httpReq.PathBase}/testzone1"
            }
        };
    });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "MultiTransfer API");
});

app.UseHttpMetrics();
app.MapMetrics("/metrics");

app.UseRateLimiter();

app.UseMiddleware<CorrelationIdMiddleware>();
//app.UseMiddleware<HmacAuthMiddleware>();
app.UseMiddleware<ExceptionMappingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();