# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.x     | Yes       |

## Reporting a Vulnerability

**Do NOT open a public issue for security vulnerabilities.**

Please report security issues to: **security@yabgu.co.uk**

Include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

We will acknowledge within 48 hours and provide a timeline for resolution.

## Security Best Practices

When deploying Hodor:

- Always set `HodorApiKey` in production
- Use TLS (HTTPS) for all endpoints
- Restrict network access to `/metrics` and `/health/info`
- Rotate API keys regularly
- Use Kubernetes NetworkPolicies or Docker network isolation
- Keep dependencies updated (Dependabot is enabled)
