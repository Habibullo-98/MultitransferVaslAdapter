namespace MultitransferVaslAdapter.Options;

public sealed class DbOptions
{
    [Required] public string ConnectionString { get; set; } = default!;
}