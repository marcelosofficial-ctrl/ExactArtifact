using System.Text;
using System.Text.Json;

namespace ExactArtifact.Core;

public sealed class ManifestService
{
    private static readonly StringComparer PathComparer =
        OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly FileHasher _fileHasher;

    public ManifestService(FileHasher? fileHasher = null)
    {
        _fileHasher = fileHasher ?? new FileHasher();
    }

    public async Task<ArtifactManifest> CreateAsync(
        string directoryPath,
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        var root = NormalizeDirectory(directoryPath);
        var outputPath = Path.GetFullPath(manifestPath);

        var files = EnumerateCanonicalFiles(root, outputPath);

        var entries = new List<ManifestFileEntry>(files.Count);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hash = await _fileHasher.ComputeDetailedAsync(
                file.FullPath,
                cancellationToken);

            entries.Add(new ManifestFileEntry
            {
                Path = file.RelativePath,
                Size = hash.Length,
                Sha256 = hash.Sha256
            });
        }

        var manifest = new ArtifactManifest
        {
            Files = entries
        };

        var json = JsonSerializer.Serialize(manifest, JsonOptions)
            .Replace("\r\n", "\n");

        if (!json.EndsWith('\n'))
        {
            json += "\n";
        }

        var parent = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        await File.WriteAllTextAsync(
            outputPath,
            json,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);

        return manifest;
    }

    public async Task<ManifestVerificationResult> VerifyAsync(
        string directoryPath,
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        var root = NormalizeDirectory(directoryPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);

        if (!File.Exists(fullManifestPath))
        {
            throw new FileNotFoundException(
                "The manifest file could not be found.",
                fullManifestPath);
        }

        ArtifactManifest manifest;

        await using (var stream = new FileStream(
            fullManifestPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            manifest = await JsonSerializer.DeserializeAsync<ArtifactManifest>(
                stream,
                JsonOptions,
                cancellationToken)
                ?? throw new InvalidDataException(
                    "The manifest is empty or invalid.");
        }

        ValidateManifest(manifest);

        var expected = new Dictionary<string, ManifestFileEntry>(
            PathComparer);

        foreach (var entry in manifest.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resolvedPath = ResolveSafeManifestPath(
                root,
                entry.Path);

            var canonicalPath = NormalizeRelativePath(
                root,
                resolvedPath);

            if (!expected.TryAdd(canonicalPath, entry))
            {
                throw new InvalidDataException(
                    $"Manifest contains a duplicate path: {entry.Path}");
            }
        }

        var actualFiles = EnumerateCanonicalFiles(
            root,
            fullManifestPath);

        var actual = actualFiles.ToDictionary(
            file => file.RelativePath,
            file => file.FullPath,
            PathComparer);

        var modified = new List<string>();
        var missing = new List<string>();
        var unexpected = new List<string>();
        var matched = 0;

        foreach (var pair in expected
            .OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = pair.Key;
            var expectedEntry = pair.Value;

            if (!actual.TryGetValue(relativePath, out var fullPath))
            {
                missing.Add(relativePath);
                continue;
            }

            var info = new FileInfo(fullPath);

            if (info.Length != expectedEntry.Size)
            {
                modified.Add(relativePath);
                continue;
            }

            var actualHash = await _fileHasher.ComputeSha256Async(
                fullPath,
                cancellationToken);

            if (!HashVerifier.IsMatch(
                actualHash,
                expectedEntry.Sha256))
            {
                modified.Add(relativePath);
                continue;
            }

            matched++;
        }

        foreach (var relativePath in actual.Keys
            .Where(path => !expected.ContainsKey(path))
            .OrderBy(path => path, StringComparer.Ordinal))
        {
            unexpected.Add(relativePath);
        }

        return new ManifestVerificationResult
        {
            ExpectedCount = expected.Count,
            MatchedCount = matched,
            Modified = modified,
            Missing = missing,
            Unexpected = unexpected
        };
    }

    private static void ValidateManifest(
        ArtifactManifest manifest)
    {
        if (manifest.FormatVersion != 1)
        {
            throw new InvalidDataException(
                $"Unsupported manifest format version: {manifest.FormatVersion}");
        }

        if (!string.Equals(
            manifest.Algorithm,
            "SHA-256",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Unsupported manifest algorithm: {manifest.Algorithm}");
        }

        foreach (var entry in manifest.Files)
        {
            if (string.IsNullOrWhiteSpace(entry.Path))
            {
                throw new InvalidDataException(
                    "Manifest contains an empty file path.");
            }

            if (entry.Size < 0)
            {
                throw new InvalidDataException(
                    $"Manifest contains an invalid size for {entry.Path}.");
            }

            try
            {
                HashVerifier.NormalizeSha256(entry.Sha256);
            }
            catch (Exception ex)
                when (ex is ArgumentException or FormatException)
            {
                throw new InvalidDataException(
                    $"Manifest contains an invalid SHA-256 for {entry.Path}.",
                    ex);
            }
        }
    }

    private static string NormalizeDirectory(
        string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var root = Path.GetFullPath(directoryPath);

        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException(
                $"Directory not found: {root}");
        }

        return root;
    }

    private static List<CanonicalFile> EnumerateCanonicalFiles(
        string root,
        string excludedFullPath)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = false,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        var excluded = Path.GetFullPath(excludedFullPath);

        return Directory
            .EnumerateFiles(root, "*", options)
            .Select(Path.GetFullPath)
            .Where(path => !PathComparer.Equals(path, excluded))
            .Select(path => new CanonicalFile(
                path,
                NormalizeRelativePath(root, path)))
            .OrderBy(
                file => file.RelativePath,
                StringComparer.Ordinal)
            .ToList();
    }

    private static string ResolveSafeManifestPath(
        string root,
        string manifestRelativePath)
    {
        if (Path.IsPathRooted(manifestRelativePath))
        {
            throw new InvalidDataException(
                $"Manifest path must be relative: {manifestRelativePath}");
        }

        var platformPath = manifestRelativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var resolved = Path.GetFullPath(
            Path.Combine(root, platformPath));

        var relative = Path.GetRelativePath(root, resolved);

        if (relative == ".." ||
            relative.StartsWith(
                ".." + Path.DirectorySeparatorChar,
                StringComparison.Ordinal) ||
            Path.IsPathRooted(relative))
        {
            throw new InvalidDataException(
                $"Manifest path escapes the target directory: {manifestRelativePath}");
        }

        return resolved;
    }

    private static string NormalizeRelativePath(
        string root,
        string fullPath)
    {
        return Path.GetRelativePath(root, fullPath)
            .Replace('\\', '/');
    }

    private sealed record CanonicalFile(
        string FullPath,
        string RelativePath);
}