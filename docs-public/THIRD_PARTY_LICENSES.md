# Third-Party Licenses

Hodor uses the following dependencies. All are **permissive** (MIT, Apache 2.0, BSD) and compatible with our MIT license. No commercial license or attribution beyond standard notices is required.

## Direct Dependencies (Hodor projects)

| Package | Version | License | Notes |
|---------|---------|---------|-------|
| Microsoft.Extensions.* | 10.0.0 | MIT | .NET runtime |
| Microsoft.EntityFrameworkCore | 10.0.0 | MIT | EF Core |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | PostgreSQL License | Permissive, MIT-like |
| Pgvector.EntityFrameworkCore | 0.3.0 | MIT | Vector support |
| Serilog.* | various | Apache 2.0 | Logging |
| DotNetEnv | 3.1.1 | MIT | .env loader |
| AspNetCore.HealthChecks.* | 9.0.0 | Apache 2.0 | Health checks |

## Samples (Rust)

| Package | Version | License |
|---------|---------|---------|
| reqwest | 0.12 | MIT OR Apache-2.0 |

## License Compatibility

- **MIT** (Hodor): Compatible with MIT, Apache 2.0, BSD, PostgreSQL License
- No GPL, AGPL, or other copyleft licenses
- No commercial or paid licenses

## Verification

To list all NuGet package licenses:

```bash
dotnet list package --include-transitive | head -50
```

Or use [nuget-license](https://www.nuget.org/packages/nuget-license) for a full report.
