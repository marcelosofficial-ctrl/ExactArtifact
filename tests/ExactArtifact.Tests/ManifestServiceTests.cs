using System.Text.Json;
using ExactArtifact.Core;

namespace ExactArtifact.Tests;

public sealed class ManifestServiceTests
{
    [Fact]
    public async Task CreateAsync_SortsPathsAndUsesForwardSlashes()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");

        Directory.CreateDirectory(
            Path.Combine(root, "nested"));

        await File.WriteAllTextAsync(
            Path.Combine(root, "z.txt"),
            "z");

        await File.WriteAllTextAsync(
            Path.Combine(root, "nested", "a.txt"),
            "a");

        var manifestPath = Path.Combine(
            temp.Path,
            "manifest.json");

        var service = new ManifestService();
        var manifest = await service.CreateAsync(
            root,
            manifestPath);

        Assert.Equal(
            new[] { "nested/a.txt", "z.txt" },
            manifest.Files.Select(file => file.Path));
    }

    [Fact]
    public async Task CreateAsync_RepeatedCreation_IsByteDeterministic()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");

        await File.WriteAllTextAsync(
            Path.Combine(root, "a.txt"),
            "alpha");

        await File.WriteAllTextAsync(
            Path.Combine(root, "b.txt"),
            "beta");

        var first = Path.Combine(temp.Path, "first.json");
        var second = Path.Combine(temp.Path, "second.json");

        var service = new ManifestService();

        await service.CreateAsync(root, first);
        await service.CreateAsync(root, second);

        var firstBytes = await File.ReadAllBytesAsync(first);
        var secondBytes = await File.ReadAllBytesAsync(second);

        Assert.Equal(firstBytes, secondBytes);
    }

    [Fact]
    public async Task CreateAsync_ManifestInsideRoot_ExcludesItself()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");

        await File.WriteAllTextAsync(
            Path.Combine(root, "a.txt"),
            "alpha");

        var manifestPath = Path.Combine(
            root,
            "ExactArtifact.manifest.json");

        var service = new ManifestService();
        var manifest = await service.CreateAsync(
            root,
            manifestPath);

        Assert.Single(manifest.Files);
        Assert.Equal("a.txt", manifest.Files[0].Path);
    }

    [Fact]
    public async Task VerifyAsync_UnchangedDirectory_IsExactMatch()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");

        await File.WriteAllTextAsync(
            Path.Combine(root, "a.txt"),
            "alpha");

        var manifestPath = Path.Combine(
            root,
            "ExactArtifact.manifest.json");

        var service = new ManifestService();
        await service.CreateAsync(root, manifestPath);

        var result = await service.VerifyAsync(
            root,
            manifestPath);

        Assert.True(result.IsExactMatch);
        Assert.Equal(1, result.MatchedCount);
    }

    [Fact]
    public async Task VerifyAsync_ModifiedFile_IsReported()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");
        var file = Path.Combine(root, "a.txt");

        await File.WriteAllTextAsync(file, "alpha");

        var manifestPath = Path.Combine(
            temp.Path,
            "manifest.json");

        var service = new ManifestService();
        await service.CreateAsync(root, manifestPath);

        await File.WriteAllTextAsync(file, "changed");

        var result = await service.VerifyAsync(
            root,
            manifestPath);

        Assert.False(result.IsExactMatch);
        Assert.Contains("a.txt", result.Modified);
    }

    [Fact]
    public async Task VerifyAsync_MissingFile_IsReported()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");
        var file = Path.Combine(root, "a.txt");

        await File.WriteAllTextAsync(file, "alpha");

        var manifestPath = Path.Combine(
            temp.Path,
            "manifest.json");

        var service = new ManifestService();
        await service.CreateAsync(root, manifestPath);

        File.Delete(file);

        var result = await service.VerifyAsync(
            root,
            manifestPath);

        Assert.False(result.IsExactMatch);
        Assert.Contains("a.txt", result.Missing);
    }

    [Fact]
    public async Task VerifyAsync_UnexpectedFile_IsReported()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");

        await File.WriteAllTextAsync(
            Path.Combine(root, "a.txt"),
            "alpha");

        var manifestPath = Path.Combine(
            temp.Path,
            "manifest.json");

        var service = new ManifestService();
        await service.CreateAsync(root, manifestPath);

        await File.WriteAllTextAsync(
            Path.Combine(root, "extra.txt"),
            "extra");

        var result = await service.VerifyAsync(
            root,
            manifestPath);

        Assert.False(result.IsExactMatch);
        Assert.Contains("extra.txt", result.Unexpected);
    }

    [Fact]
    public async Task VerifyAsync_PathTraversalManifest_IsRejected()
    {
        using var temp = new TempDirectory();
        var root = temp.CreateDirectory("payload");

        var manifestPath = Path.Combine(
            temp.Path,
            "manifest.json");

        var manifest = new
        {
            formatVersion = 1,
            algorithm = "SHA-256",
            files = new[]
            {
                new
                {
                    path = "../escape.txt",
                    size = 1,
                    sha256 = new string('0', 64)
                }
            }
        };

        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(manifest));

        var service = new ManifestService();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => service.VerifyAsync(
                root,
                manifestPath));
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } =
            System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "ExactArtifact.Tests",
                Guid.NewGuid().ToString("N"));

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public string CreateDirectory(string name)
        {
            var path = System.IO.Path.Combine(Path, name);
            Directory.CreateDirectory(path);
            return path;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
            }
        }
    }
}