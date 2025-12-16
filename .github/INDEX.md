# GitHub Configuration

This directory contains GitHub-specific configuration files for the DbDiff project.

## Contents

### Workflows (`.github/workflows/`)

Automated CI/CD pipelines:

- **`ci.yml`** - Continuous Integration
  - Runs on: Push and PR to `main`/`develop`
  - Actions: Build, test, upload artifacts
  
- **`pr-check.yml`** - Pull Request Validation
  - Runs on: Pull requests
  - Actions: Multi-platform testing (Linux, Windows, macOS)
  
- **`release.yml`** - Automated Releases
  - Runs on: Version tags (`v*.*.*`)
  - Actions: Build binaries, create releases

See [WORKFLOWS.md](WORKFLOWS.md) for detailed documentation.

### Issue Templates (`.github/ISSUE_TEMPLATE/`)

- **`bug_report.md`** - Template for reporting bugs
- **`feature_request.md`** - Template for requesting features

### Pull Request Template

- **`PULL_REQUEST_TEMPLATE.md`** - Standard PR template with checklist

### Dependabot Configuration

- **`dependabot.yml`** - Automated dependency updates
  - NuGet packages: Weekly on Mondays
  - GitHub Actions: Weekly on Mondays

## Quick Start

### For Contributors

1. Read [CONTRIBUTING.md](../CONTRIBUTING.md)
2. Use the appropriate issue template when reporting issues
3. Follow the PR template when submitting changes
4. Ensure all CI checks pass

### For Maintainers

#### Creating a Release

```bash
# 1. Update CHANGELOG.md
# 2. Commit changes
git commit -am "Release v1.0.0"

# 3. Create and push tag
git tag v1.0.0
git push origin v1.0.0

# 4. GitHub Actions handles the rest!
```

#### Monitoring Workflows

- Go to the **Actions** tab in the repository
- Click on a workflow to see its runs
- Click on a run to see detailed logs

## Badges

Add these to your README.md:

```markdown
[![CI](https://github.com/olibf/dbdiff/actions/workflows/ci.yml/badge.svg)](https://github.com/olibf/dbdiff/actions/workflows/ci.yml)
[![Release](https://github.com/olibf/dbdiff/actions/workflows/release.yml/badge.svg)](https://github.com/olibf/dbdiff/actions/workflows/release.yml)
```

## Permissions

Workflows require the following repository permissions:

- **Contents**: Write (for creating releases)
- **Actions**: Read (for workflow execution)

Configure in: Settings → Actions → General → Workflow permissions

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [Issue Templates Guide](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests)

