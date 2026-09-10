# ExactArtifact

A small local-first utility for proving that a file is exactly the file you expected.

ExactArtifact uses streaming SHA-256 hashing so large files do not need to be loaded fully into memory.

## Current commands

```powershell
exactartifact hash <file>
exactartifact verify <file> <expected-sha256>
```

## Design goals

- Small and understandable
- Local-first
- Predictable memory usage
- Useful from both humans and scripts
- Strong validation and tests
- No unnecessary dependencies

## Status

Current milestone: EA-01.