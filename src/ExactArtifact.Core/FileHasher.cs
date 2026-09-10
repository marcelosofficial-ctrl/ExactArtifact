using System.Security.Cryptography;

namespace ExactArtifact.Core;

public sealed record FileHashResult(
    string Sha256,
    long Length,
    DateTime LastWriteTimeUtc);

public sealed class FileHasher
{
    private const int BufferSize = 1024 * 1024;

    public async Task<string> ComputeSha256Async(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var result = await ComputeDetailedAsync(filePath, cancellationToken);
        return result.Sha256;
    }

    public async Task<FileHashResult> ComputeDetailedAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fullPath = Path.GetFullPath(filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "The file to hash could not be found.",
                fullPath);
        }

        var before = new FileInfo(fullPath);
        var beforeLength = before.Length;
        var beforeWrite = before.LastWriteTimeUtc;

        await using var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (stream.Length != beforeLength)
        {
            throw new IOException(
                "The file changed before hashing could begin.");
        }

        using var sha256 = SHA256.Create();

        var hash = await sha256.ComputeHashAsync(
            stream,
            cancellationToken);

        var streamLengthAfterHash = stream.Length;

        var after = new FileInfo(fullPath);
        after.Refresh();

        if (streamLengthAfterHash != beforeLength ||
            after.Length != beforeLength ||
            after.LastWriteTimeUtc != beforeWrite)
        {
            throw new IOException(
                "The file changed while it was being hashed.");
        }

        return new FileHashResult(
            Convert.ToHexString(hash).ToLowerInvariant(),
            beforeLength,
            beforeWrite);
    }
}