namespace Deploy.Helpers;

public static class ProfileHelpers
{
    private static readonly string[] Prefixes =
        ["TIG", "FOX", "EAG", "OWL", "BEA", "WLF", "LNX", "DEE", "HAR", "OTR"];

    /// <summary>
    /// Generates a unique profile code using a random 3-letter prefix
    /// and 4 random digits, e.g. "TIG-4821".
    /// </summary>
    public static string GenerateProfileCode()
    {
        var prefix = Prefixes[Random.Shared.Next(Prefixes.Length)];
        var digits = Random.Shared.Next(1000, 9999);
        return $"{prefix}-{digits}";
    }

    /// <summary>
    /// Generates a URL-safe, Base64-encoded session token derived from a new <see cref="Guid"/>.
    /// </summary>
    public static string GenerateSessionToken()
        => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
               .TrimEnd('=')
               .Replace('+', '-')
               .Replace('/', '_');
}
