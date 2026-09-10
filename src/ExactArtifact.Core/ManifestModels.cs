using System.Text.Json.Serialization;

namespace ExactArtifact.Core;

public sealed class ArtifactManifest
{
    [JsonPropertyOrder(0)]
    public int FormatVersion { get; init; } = 1;

    [JsonPropertyOrder(1)]
    public string Algorithm { get; init; } = "SHA-256";

    [JsonPropertyOrder(2)]
    public List<ManifestFileEntry> Files { get; init; } = [];
}

public sealed class ManifestFileEntry
{
    [JsonPropertyOrder(0)]
    public required string Path { get; init; }

    [JsonPropertyOrder(1)]
    public long Size { get; init; }

    [JsonPropertyOrder(2)]
    public required string Sha256 { get; init; }
}

public sealed class ManifestVerificationResult
{
    public int ExpectedCount { get; init; }
    public int MatchedCount { get; init; }
    public List<string> Modified { get; init; } = [];
    public List<string> Missing { get; init; } = [];
    public List<string> Unexpected { get; init; } = [];

    public bool IsExactMatch =>
        Modified.Count == 0 &&
        Missing.Count == 0 &&
        Unexpected.Count == 0 &&
        MatchedCount == ExpectedCount;
}