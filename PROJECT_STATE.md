# ExactArtifact Project State

## Purpose

ExactArtifact is a small local-first Windows file-integrity utility for proving that files and release directories contain exactly the expected bytes.

## Portfolio role

Focused Project: small, finished, intentional, and easy to understand.

## 1.0 milestones

EA-01: Streaming SHA-256 core, CLI, and tests. COMPLETE.
EA-02: Deterministic manifest creation and verification plus defensive file handling. COMPLETE.
EA-03: Lightweight native Windows GUI. COMPLETE.
EA-04: Benchmarking, documentation, packaging, and release validation. COMPLETE.
EA-1.0: GitHub publication, release upload, and portfolio entry. NOT YET PUBLISHED.

## Core capabilities

- Streaming SHA-256
- Expected hash verification
- Strict normalized SHA-256 validation
- Cancellation
- File mutation checks
- Deterministic JSON manifests
- Canonical relative paths
- Stable ordering
- Self-excluding manifests
- Matched / modified / missing / unexpected classification
- Path traversal rejection
- Reparse-point exclusion
- Script-friendly exit codes

## Interfaces

- ExactArtifact.Gui: native WPF GUI
- ExactArtifact.Cli: automation-friendly CLI
- Both consume ExactArtifact.Core

## Local validation

Validated: 2026-09-11 05:19:47 +09:00

Release solution build: PASS
Automated tests: 22/22 PASS
Single-file CLI regression: PASS
Manifest regression: PASS
Development GUI launch: PASS
Self-contained release GUI launch: PASS
Clean ZIP extraction GUI launch: PASS
Clean ZIP extraction CLI smoke test: PASS

## Benchmark

64 MiB:
- Time: 0.115 s
- Throughput: 554.7 MiB/s
- Peak working set: 25.9 MiB

512 MiB:
- Time: 0.4 s
- Throughput: 1280.1 MiB/s
- Peak working set: 29.5 MiB

Input-size increase: 8x
Peak-working-set change: 3.6 MiB

## Release packaging decision

Version 1.0.0 uses:

- GUI: self-contained multi-file win-x64 publish
- CLI: self-contained single-file win-x64 publish

The GUI single-file publish path was tested and rejected because it exited immediately under the current .NET 10 WPF SDK. The conservative multi-file self-contained GUI package was then validated from both the publish directory and a clean extracted ZIP.

## Local release candidate

ZIP: artifacts/release/ExactArtifact-1.0.0-win-x64.zip
ZIP SHA-256: 85ea37266d726e7c43487fa2da9b493428be001fbb3c616ec6742180268cd66a
ZIP size: 90.7 MiB

## GitHub status

No GitHub Actions were used.
No GitHub publication has happened yet.

## Next milestone

EA-1.0 publication only:

1. Review local release candidate.
2. Publish repository.
3. Publish v1.0.0 release using tested ZIP and SHA256SUMS.txt.
4. Optionally add manual-only CI later.
5. Add ExactArtifact to portfolio Focused Projects.
6. Stop 1.0 feature development.