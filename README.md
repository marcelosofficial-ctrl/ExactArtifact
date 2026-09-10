# ExactArtifact

A small local-first utility for proving that a file or release directory is exactly what you expected.

ExactArtifact uses streaming SHA-256 hashing so large files do not need to be loaded fully into memory.

## Commands

```powershell
exactartifact hash <file>
exactartifact verify <file> <expected-sha256>

exactartifact manifest create <directory> <manifest-file>
exactartifact manifest verify <directory> <manifest-file>
```

## Manifest design

ExactArtifact manifests are deterministic JSON.

They contain only the data required to identify the files:

- relative path
- byte size
- SHA-256

Entries are sorted by canonical relative path. Generated manifests contain no timestamps, machine names, absolute paths, or other environment-specific data.

If the manifest is stored inside the directory being described, the manifest excludes itself.

Manifest verification reports:

- matched files
- modified files
- missing files
- unexpected files

Manifest paths are constrained to the target directory so a malicious or malformed manifest cannot escape the verification root with `..` traversal.

## Design goals

- Small and understandable
- Local-first
- Predictable memory usage
- Deterministic output
- Useful from both humans and scripts
- Strong validation and tests
- No unnecessary dependencies

## Status

EA-02.