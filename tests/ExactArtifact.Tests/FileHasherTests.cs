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
}