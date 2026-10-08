namespace MultitransferVaslAdapter.Common;

public static class LogMaskingExtensions
{
    public static string MaskPhone(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 4) return "***";
        return $"{value[..2]}***{value[^2..]}";
    }

    public static string MaskAccount(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 6) return "***";
        return $"{value[..4]}***{value[^2..]}";
    }
}
