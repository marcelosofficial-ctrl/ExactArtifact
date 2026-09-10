# ExactArtifact Project State

## Purpose

ExactArtifact is a small local-first file-integrity utility for proving that files and release artifacts contain exactly the expected bytes.

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

- ExactArtifact.Core: shared integrity and verification logic.
- ExactArtifact.Cli: command-line interface consuming Core.
- ExactArtifact.Tests: independent tests of Core.
- Future GUI: consumes the same Core library.

## Engineering principles

- Stream files rather than loading them fully into memory.
- Memory use must not meaningfully scale with file size.
- Keep behavior deterministic and automation-friendly.
- Fail clearly.
- Keep 1.0 scope disciplined.

## Current milestone

EA-01 COMPLETE.

## EA-01 implemented

- .NET 10 solution
- Shared Core library
- Streaming SHA-256 hashing
- Hash normalization and comparison
- CLI hash command
- CLI verify command
- Script-friendly exit codes
- Automated xUnit tests
- End-to-end MATCH and MISMATCH validation
- Local Release build
- Local Git history

## Validation

Last validated locally: 2026-09-11 04:40:59 +09:00

Release build: PASS
Automated tests: 8/8 PASS
CLI hash: PASS
CLI MATCH verification: PASS
CLI MISMATCH verification: PASS

## GitHub policy

Normal development and validation are local.
No GitHub Actions are required during development.
Publication happens only when locally release-ready.
Initial CI, if added, should use manual workflow_dispatch.

## Next milestone

EA-02: deterministic manifests plus defensive file handling.

## GitHub status

No GitHub operation was performed by this batch.