# ExactArtifact 1.0.0 release notes

## Included

- Native Windows GUI
- Streaming SHA-256 file hashing
- Expected-hash comparison
- Deterministic folder manifests
- Modified, missing, and unexpected file detection
- Script-friendly CLI
- Cancellation support
- Local-only operation
- No accounts, telemetry, or cloud dependency

## Windows packaging

The GUI is published as a self-contained multi-file Windows x64 application.

This is intentional. The current .NET 10 WPF toolchain has an upstream regression affecting some self-contained single-file WPF applications at startup. ExactArtifact therefore uses the more conservative self-contained multi-file GUI packaging path for 1.0.0.

The CLI remains a self-contained single executable at:

`cli\exactartifact.exe`

No .NET installation is required for the release package.