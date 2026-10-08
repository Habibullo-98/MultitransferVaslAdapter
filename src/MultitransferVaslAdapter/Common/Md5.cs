namespace MultitransferVaslAdapter.Common;

public static class Md5
{
    public static string HexLower(string value)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}