namespace MultitransferVaslAdapter.Options;

public sealed class VaslOptions
{
    [Required] public string BaseUrl { get; set; } = default!;
    [Required] public string SecretKey { get; set; } = default!;
    [Range(1, int.MaxValue)] public int PartnerId { get; set; }
    [Range(1, int.MaxValue)] public int ServiceId { get; set; }
    [Range(1, 120)] public int TimeoutSeconds { get; set; } = 10;
}
