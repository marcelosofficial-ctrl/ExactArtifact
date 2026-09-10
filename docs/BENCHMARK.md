# Local benchmark

Benchmark date: 2026-09-11
Build: Release, .NET 10
Algorithm: SHA-256

| Input | Time | Throughput | Peak working set |
| --- | ---: | ---: | ---: |
| 64 MiB | 0.115 s | 554.7 MiB/s | 25.9 MiB |
| 512 MiB | 0.4 s | 1280.1 MiB/s | 29.5 MiB |

Input size increased 8x.

Observed peak-working-set change: 3.6 MiB.

These are measurements from one local machine and are not universal performance claims. ExactArtifact streams data through a fixed-size hashing path instead of loading the whole artifact into memory.