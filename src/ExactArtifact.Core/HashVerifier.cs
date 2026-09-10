namespace ExactArtifact.Core;

public static class HashVerifier
{
    public static bool IsMatch(
        string calculatedHash,
        string expectedHash)
    {
        var calculated = NormalizeSha256(calculatedHash);
        var expected = NormalizeSha256(expectedHash);

        return string.Equals(
            calculated,
            expected,
            StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeSha256(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);

        var normalized = hash
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);

        if (normalized.Length != 64 ||
            !normalized.All(Uri.IsHexDigit))
        {
            throw new FormatException(
                "A SHA-256 hash must contain exactly 64 hexadecimal characters.");
        }

        return normalized.ToLowerInvariant();
    }
}