# Contributing to Hodor

Thank you for your interest in contributing to the Hodor MCP API Gateway!

## Repository Rules

| Action | @kursatarslan | Contributors |
|--------|---------------|--------------|
| Open PR | Yes | Yes |
| Approve PR | Yes | No (CODEOWNERS) |
| Merge to main | Yes | No |
| Direct push to main | Yes | No |

All PRs require approval from **@kursatarslan** before merge.

## How to Contribute

1. **Fork** the repository
2. **Create a branch** from `main`:
   - `feat/description` — new feature
   - `fix/description` — bug fix
   - `docs/description` — documentation
   - `infra/description` — Docker, K8s, CI
   - `refactor/description` — code quality
3. **Make changes** following the code style
4. **Run tests**: `dotnet test`
5. **Format code**: `dotnet format`
6. **Commit** with conventional messages:
   - `feat: add pagination to tools/list`
   - `fix: resolve circuit breaker race condition`
   - `docs: update API endpoint table`
7. **Push** and **open a Pull Request**

## Development Setup

```bash
git clone https://github.com/Hodor-Mcp-Apigateway/hodor.git
cd hodor

# Build
dotnet restore Hodor.slnx
dotnet build Hodor.slnx

# Test
dotnet test Hodor.slnx

# Run with Docker (PostgreSQL + Hodor)
make docker-compose-up

# Or run locally (requires PostgreSQL)
make run
```

### Local Overrides (optional, gitignored)

- **`Directory.Build.props.user`** — Local MSBuild overrides. Copy from `Directory.Build.props.user.example`.
- **User-level NuGet** — Private feeds go in `~/.nuget/NuGet/NuGet.Config`.

## Code Style

- Follow `.editorconfig`
- Run `dotnet format` before committing
- Analyzers: Meziantou, Roslynator, CSharpGuidelines (all configured)
- No `Console.WriteLine` — use `ILogger`
- Async methods end with `Async`

## Pull Request Checklist

- [ ] Branch name follows convention (`feat/`, `fix/`, `docs/`, etc.)
- [ ] Commit messages are descriptive
- [ ] All tests pass
- [ ] Code formatted (`dotnet format`)
- [ ] No secrets committed
- [ ] Documentation updated (if applicable)
- [ ] PR description explains what and why

## CI/CD

Every PR triggers:
- Build (Release)
- Unit tests with coverage
- Docker build verification
- Code format check

Merging to `main` triggers release draft update.
Pushing a `v*` tag triggers Docker image build.
