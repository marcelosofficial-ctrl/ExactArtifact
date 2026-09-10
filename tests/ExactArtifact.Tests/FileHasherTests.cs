using ExactArtifact.Core;

namespace ExactArtifact.Tests;

public sealed class FileHasherTests
{
    [Fact]
    public async Task ComputeSha256Async_KnownContent_ReturnsExpectedHash()
    {
        var path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "hello world");

            var hasher = new FileHasher();
            var result = await hasher.ComputeSha256Async(path);

            Assert.Equal(
                "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
                result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ComputeSha256Async_MissingFile_ThrowsFileNotFoundException()
    {
        var hasher = new FileHasher();

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => hasher.ComputeSha256Async(
                Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString())));
    }

    [Fact]
    public async Task ComputeSha256Async_EmptyFile_ReturnsKnownHash()
    {
        var path = Path.GetTempFileName();

        try
        {
            await File.WriteAllBytesAsync(path, []);

            var hasher = new FileHasher();
            var result = await hasher.ComputeSha256Async(path);

            Assert.Equal(
                "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ComputeDetailedAsync_ReturnsLengthAndHash()
    {
        var path = Path.GetTempFileName();

        try
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            await File.WriteAllBytesAsync(path, bytes);

            var hasher = new FileHasher();
            var result = await hasher.ComputeDetailedAsync(path);

            Assert.Equal(5, result.Length);
            Assert.Equal(64, result.Sha256.Length);
        }
        finally
        {
            File.Delete(path);
        }
    }
}