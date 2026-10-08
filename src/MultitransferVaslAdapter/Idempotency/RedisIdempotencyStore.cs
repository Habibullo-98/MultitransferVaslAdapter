using StackExchange.Redis;
using MultitransferVaslAdapter.Options;

namespace MultitransferVaslAdapter.Idempotency;

public interface IIdempotencyStore
{
    Task<string?> GetAsync(string key, CancellationToken ct);
    Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken ct);
}

public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly Lazy<ConnectionMultiplexer> _mux;

    public RedisIdempotencyStore(IOptions<RedisOptions> opt)
    {
        _mux = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(opt.Value.ConnectionString));
    }

    public async Task<string?> GetAsync(string key, CancellationToken ct)
    {
        var db = _mux.Value.GetDatabase();
        var val = await db.StringGetAsync(key);
        return val.HasValue ? val.ToString() : null;
    }

    public async Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken ct)
    {
        var db = _mux.Value.GetDatabase();
        await db.StringSetAsync(key, value, ttl);
    }
}
