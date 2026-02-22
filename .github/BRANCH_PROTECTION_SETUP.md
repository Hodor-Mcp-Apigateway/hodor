# Branch Protection & PR Rules

This document describes GitHub settings so that **only kursatarslan** can merge to the `main`/`master` branch and approve PRs in the Hodor repo.

## Step 1: Branch Protection Rule

1. Repo → **Settings** → **Branches**
2. **Add branch protection rule** or **Add rule**
3. **Branch name pattern:** `main` (or `master` if you use it)

### Recommended Settings

| Setting | Value | Description |
|---------|-------|-------------|
| Require a pull request before merging | ✅ | PR required |
| Require approvals | ✅, **1** | At least 1 approval required |
| Dismiss stale pull request approvals when new commits are pushed | ✅ | Approvals dismissed on new commits |
| Require review from Code Owners | ✅ | CODEOWNERS must approve |
| Require status checks to pass before merging | ✅ (optional) | CI must pass |
| Require branches to be up to date before merging | ✅ (optional) | Must be up to date with main |
| Do not allow bypassing the above settings | ✅ | No bypass, including admins |
| Restrict who can push to matching branches | ✅ | Only designated users |

### Restrict Push (Important)

- **Restrict who can push to matching branches** → **Add** → add `kursatarslan`
- Only **kursatarslan** can push or merge directly to `main`
- Other developers can only open PRs, not merge

## Step 2: CODEOWNERS

The `.github/CODEOWNERS` file already exists:
```
* @kursatarslan
```
All files require PR approval from **kursatarslan**.

## Step 3: Collaborators / Team (For Organization)

If the repo is under the **Hodor-Mcp-Apigateway** organization:

1. **Settings** → **Collaborators and teams** (or **Manage access**)
2. Grant **Write** or **Triage** to other developers (so they can open PRs)
3. Keep **Admin** only for yourself

## Summary

| Action | kursatarslan | Other developers |
|--------|--------------|------------------|
| Open PR | ✅ | ✅ |
| Approve PR | ✅ | ❌ (CODEOWNERS) |
| Merge to main | ✅ | ❌ (Restrict push) |
| Direct push to main | ✅ | ❌ |

## References

- [GitHub: Managing a branch protection rule](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-a-branch-protection-rule)
- [GitHub: About code owners](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners)
