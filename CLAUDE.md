# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

WhoisClient.NET is a .NET class library (NuGet package `WhoisClient.NET`, assembly `WhoisClient`, namespace `Whois.NET`) that queries WHOIS servers by IP address or domain name. It starts from `whois.iana.org`, follows referral servers recursively, and parses the final raw response into strongly typed values.

## Commands

```sh
# Build
dotnet build

# Run all tests (same as CI). The test project uses the NUnit runner on Microsoft.Testing.Platform, so it runs as an executable.
dotnet run --project WhoisClient.NET.Test -f net9.0

# Run a single test or a subset
dotnet run --project WhoisClient.NET.Test -f net9.0 -- --filter "FullyQualifiedName~Empty_Refer_Should_Not_Follow"

# Create the NuGet package (Release builds pack automatically into ./_dist)
dotnet build WhoisClient.NET -c Release
```

The test project also targets `net462`; CI (`.github/workflows/unit-tests.yml`) runs only `net9.0` on Ubuntu.

`WhoisClient.NET.TestDrive` is a separate solution that references the **published package** `WhoisClient.NET` (not the project). `nuget.config` adds `_dist` as a package source, so a locally packed Release build can be consumed there by bumping the version.

## Architecture

All logic lives in two files under `WhoisClient.NET/`.

- `WhoisClient.cs`: static API. Public `Query` / `QueryAsync` / `RawQuery` / `RawQueryAsync` each have a current overload taking `WhoisQueryOptions` and an `[Obsolete]` overload with individual parameters that just builds a `WhoisQueryOptions`. Every sync method has an async twin with duplicated logic; **changes must be applied to both**.
  - `QueryRecursive(Async)`: queries the last server in the list (retrying while the response is blank, up to `Retries`), then `HasReferral` decides whether to append a referral server and recurse. The visited host list becomes `WhoisResponse.RespondedServers`.
  - `_hasReferralRegex`: the single regex that detects referrals (`ReferralServer: whois://`, `Whois Server:`, `refer:`, `whois:`, `remarks: ... whois.xxx`). Most bug fixes land here (e.g. ignoring `http(s)://` values, not letting an empty `refer:` capture the next line). Referral to the same host stops recursion.
  - `GetQueryStatement`: per-server query syntax (`domain X` for Verisign/InterNIC, `n + X` for ARIN).
  - `RawQueryAsync` wraps each socket write/read in `InvokeAsync`, which converts timeouts into `TimeoutException`. On .NET Framework (non-`NETCOREAPP`) cancellation tokens don't work with `NetworkStream`, so it falls back to `Task.WhenAny` with delay tasks.
  - Response bytes are accumulated fully before decoding with `options.Encoding`, so multi-byte encodings (e.g. ISO-2022-JP from JPNIC) aren't split.
  - Transport errors are swallowed (returning empty/partial text) unless `RethrowExceptions` is true.
  - `WhoisQueryOptions.ConnectAsync` lets callers supply their own `TcpClient` factory (e.g. SOCKS proxy); otherwise `DefaultConnectAsync` is used.
- `WhoisResponse.cs`: constructor parses `Raw` with regexes to fill `OrganizationName` and `AddressRange` (an `IPAddressRange` from the `IPAddressRange` package). It handles JPNIC Japanese-label formats (with and without `a.`/`f.` key letters) and a special ARIN multi-network format.

`IQueryOptions` is an internal interface implemented by the public `WhoisQueryOptions`.

## Tests

- `WhoisResponseTest`: offline parsing tests against canned raw responses. Add a case here when changing `WhoisResponse` parsing.
- `IrregularCaseTests`: offline tests using `Internals/MockServer` (a loopback `TcpListener` with a per-client handler) for timeouts, retries, encoding, and referral-regex edge cases. Use this pattern for referral/transport bugs.
- `IpTests` / `DomainTests`: hit real WHOIS servers over the network. They can fail due to rate limiting (e.g. MarkMonitor) or upstream data changes, not only code regressions.

## Conventions

- The library targets `netstandard2.0` plus `net6.0`–`net9.0` with no explicit `LangVersion` and nullable disabled. The `netstandard2.0` build compiles as C# 7.3, so avoid newer syntax and guard newer BCL APIs with `#if`. The test project uses `LangVersion 10` with nullable enabled and nullable warnings as errors.
- Public members need XML doc comments (`GenerateDocumentationFile` is on).
- Release notes go in `RELEASE-NOTES.txt` (newest first, `v.X.Y.Z` headings). The csproj extracts the top section into `PackageReleaseNotes` at pack time, so don't edit `PackageReleaseNotes` in the csproj. Bump `<Version>` in `WhoisClient.NET.csproj` for releases.
