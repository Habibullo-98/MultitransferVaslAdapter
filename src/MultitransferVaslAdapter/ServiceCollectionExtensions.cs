namespace MultitransferVaslAdapter;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdapterServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddOptions<HmacOptions>().Bind(cfg.GetSection("Hmac")).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<DbOptions>().Bind(cfg.GetSection("Db")).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<RedisOptions>().Bind(cfg.GetSection("Redis")).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<VaslOptions>().Bind(cfg.GetSection("Vasl")).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<RateOptions>().Bind(cfg.GetSection("Rates")).ValidateOnStart();

        services.AddDbContext<AdapterDbContext>((sp, opt) =>
        {
            var db = sp.GetRequiredService<IOptions<DbOptions>>().Value;
            Console.WriteLine($"Using connection string: {db.ConnectionString}");
            opt.UseNpgsql(db.ConnectionString);
        });

        services.AddHttpClient("vasl")
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(2)))
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        services.AddScoped<IVaslGateway, VaslGateway>();

        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IRateService, RateService>();

        return services;
    }
}