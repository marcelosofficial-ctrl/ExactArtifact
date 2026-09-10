using ExactArtifact.Core;

namespace ExactArtifact.Tests;

public sealed class CancellationTests
{
    [Fact]
    public async Task FileHasher_PreCancelledToken_Cancels()
    {
        var path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "cancel me");

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var hasher = new FileHasher();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => hasher.ComputeSha256Async(path, cts.Token));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ManifestCreate_PreCancelledToken_Cancels()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "ExactArtifact-Cancel-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "a.txt"),
                "cancel me");

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var service = new ManifestService();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.CreateAsync(
                    root,
                    Path.Combine(root, "manifest.json"),
                    cts.Token));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}