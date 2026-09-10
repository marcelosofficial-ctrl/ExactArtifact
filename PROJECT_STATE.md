# ExactArtifact Project State

## Purpose

ExactArtifact is a small local-first file-integrity utility for proving that files and release directories contain exactly the expected bytes.

## Portfolio role

Focused Project: small, finished, intentional, and easy to understand.

## Scope for 1.0

EA-01: Streaming SHA-256 core, CLI, and tests.
EA-02: Deterministic manifest creation and verification plus defensive file handling.
EA-03: Small polished Windows GUI.
EA-04: Benchmarking, documentation, release validation, and packaging.
EA-1.0: Publish repository, release, and portfolio entry.

## Non-goals for 1.0

Cloud storage, accounts, AI, databases, malware scanning, antivirus functionality, synchronization, encryption platforms, automatic updates, or large settings systems.

## Architecture

- ExactArtifact.Core: shared hashing and manifest logic.
- ExactArtifact.Cli: command-line interface consuming Core.
- ExactArtifact.Tests: independent tests of Core.
- Future GUI: consumes the same Core library.

## Engineering principles

- Stream files rather than loading them fully into memory.
- Memory use must not meaningfully scale with file size.
- Keep manifest output deterministic.
- Keep paths portable and relative.
- Reject manifest path traversal.
- Detect obvious file mutation during hashing.
- Keep behavior automation-friendly.
- Keep 1.0 scope disciplined.

## Current milestone

EA-02 COMPLETE.

## EA-01 completed

- Streaming SHA-256
- Single-file hash and verify commands
- SHA-256 normalization and validation
- Script-friendly exit codes
- Local automated tests

## EA-02 completed

- Deterministic JSON manifests
- Canonical forward-slash relative paths
- Stable ordinal file ordering
- File byte sizes and SHA-256 values
- Self-exclusion when manifest is stored inside target directory
- Directory verification
- Matched / modified / missing / unexpected classification
- Safe manifest path resolution
- Rejection of path traversal outside verification root
- Reparse-point exclusion during recursive enumeration
- File length and modification-time checks around hashing
- CLI manifest create command
- CLI manifest verify command
- Dedicated manifest mismatch exit code 4
- Repeated-output determinism smoke validation

## Validation

Last validated locally: 2026-09-11 04:55:53 +09:00

Release build: PASS
Automated tests: 20/20 PASS
Single-file CLI behavior: retained from EA-01
Manifest create: PASS
Repeated manifest byte determinism: PASS
Manifest exact verification: PASS
Manifest mismatch detection: PASS
GitHub operations: NONE

## GitHub policy

Normal development and validation remain local.
No GitHub Actions are required during development.
Publication happens only when locally release-ready.
Initial CI, if added, should use manual workflow_dispatch.

## Next milestone

EA-03: small polished Windows GUI using the existing Core library.

## GitHub status

No GitHub operation was performed by EA-02.