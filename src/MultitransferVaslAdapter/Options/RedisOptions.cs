namespace MultitransferVaslAdapter.Options;

public sealed class RedisOptions
{
    [Required] public string ConnectionString { get; set; } = default!;
}
