namespace MultitransferVaslAdapter.Options;

public sealed class HmacOptions
{
    [Required] public string ApiKey { get; set; } = default!;
    [Required] public string Secret { get; set; } = default!;
    public int AllowedSkewSeconds { get; set; } = 300;
}
