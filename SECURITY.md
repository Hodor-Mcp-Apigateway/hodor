# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Reporting a Vulnerability

Please report security vulnerabilities by opening a [GitHub Security Advisory](https://github.com/YOUR_ORG/hodor/security/advisories/new).

Do **not** open a public issue for security vulnerabilities.

## Security Considerations

- Do not commit `.env` files or connection strings with credentials
- Use environment variables or secrets management for production
- PostgreSQL connection strings may contain passwords—keep them secure
