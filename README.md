# ExactArtifact

ExactArtifact is a small local-first Windows utility for proving that a file or release directory contains exactly the bytes you expected.

It provides both a native Windows interface and a script-friendly CLI. Both use the same shared integrity engine.

## What it does

### Single files

- Stream SHA-256 without loading the entire file into memory
- Copy the calculated hash
- Compare against an expected SHA-256
- Normalize uppercase, lowercase, spaced, or hyphenated SHA-256 input
- Cancel long-running GUI operations
- Detect obvious file mutation around the hashing operation

### Release folders

ExactArtifact creates deterministic JSON manifests containing:

- canonical relative path
- byte size
- SHA-256

Verification classifies files as matched, modified, missing, or unexpected.

Generated manifests intentionally omit timestamps, machine names, absolute paths, and other environment-specific metadata. Entries are sorted deterministically and the manifest excludes itself when stored inside the target directory.

Manifest path traversal outside the selected root is rejected.

## Windows app

Run:

`ExactArtifact.exe`

The Windows x64 release is self-contained, so users do not need to install .NET.

The GUI is intentionally shipped as a self-contained multi-file application rather than a WPF single-file bundle because of a current upstream .NET 10 WPF single-file startup regression.

## CLI

The portable CLI is:

`cli\exactartifact.exe`

Usage:

```powershell
exactartifact hash <file>
exactartifact verify <file> <expected-sha256>

exactartifact manifest create <directory> <manifest-file>
exactartifact manifest verify <directory> <manifest-file>
```

Exit codes:

| Code | Meaning |
| ---: | --- |
| 0 | Success / exact match |
| 1 | Invalid input or operational error |
| 2 | Cancelled |
| 3 | Single-file hash mismatch |
| 4 | Manifest mismatch |

## Architecture

```text
ExactArtifact.Gui ─┐
                   ├─> ExactArtifact.Core
ExactArtifact.Cli ─┘

ExactArtifact.Tests ─> ExactArtifact.Core
```

Integrity logic stays in `ExactArtifact.Core`. The GUI and CLI are thin interfaces over the same implementation.

## Engineering goals

- Small and understandable
- Local-first
- Predictable memory use
- Deterministic output
- Defensive path handling
- Automation-friendly CLI behavior
- No unnecessary dependencies

## Build locally

Requires the .NET 10 SDK.

```powershell
dotnet build ExactArtifact.slnx --configuration Release
dotnet test tests\ExactArtifact.Tests\ExactArtifact.Tests.csproj --configuration Release
```

## Benchmark

See `docs/BENCHMARK.md`.

## License

MIT.