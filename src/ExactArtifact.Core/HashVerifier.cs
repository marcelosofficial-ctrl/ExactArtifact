namespace ExactArtifact.Core;

public static class HashVerifier
{
    public static bool IsMatch(
        string calculatedHash,
        string expectedHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(calculatedHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedHash);

        var calculated = Normalize(calculatedHash);
        var expected = Normalize(expectedHash);

        return string.Equals(
            calculated,
            expected,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string hash)
    {
        return hash
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);
    }
}