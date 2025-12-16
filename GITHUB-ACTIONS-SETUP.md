# GitHub Actions Setup Summary

This document summarizes the GitHub Actions CI/CD configuration that has been set up for the DbDiff project.

## What Was Configured

### 1. Continuous Integration (CI)

**File:** `.github/workflows/ci.yml`

**Purpose:** Automatically build and test the application on every push and pull request.

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**What it does:**
1. Checks out the code
2. Sets up .NET 10.0
3. Restores NuGet dependencies
4. Builds the project in Release mode
5. Runs all unit tests
6. Uploads build artifacts (retained for 7 days)

**Benefits:**
- Catches build errors immediately
- Ensures all tests pass before merging
- Provides early feedback to developers

### 2. Pull Request Validation

**File:** `.github/workflows/pr-check.yml`

**Purpose:** Test the application on multiple platforms before merging pull requests.

**Platforms:**
- Ubuntu (Linux)
- Windows
- macOS

**What it does:**
1. Runs build and test on all three platforms
2. Verifies that publishing succeeds for each platform
3. Ensures cross-platform compatibility

**Benefits:**
- Catches platform-specific issues early
- Validates that releases will work on all platforms
- Increases confidence in pull requests

### 3. Automated Releases

**File:** `.github/workflows/release.yml`

**Purpose:** Automatically build and publish releases when version tags are pushed.

**Triggers:**
- Push of tags matching `v*.*.*` (e.g., `v1.0.0`, `v2.1.3`)

**What it does:**
1. Extracts version number from tag
2. Builds self-contained executables for:
   - Windows x64
   - Linux x64
   - macOS x64 (Intel)
   - macOS ARM64 (Apple Silicon)
3. Creates ZIP (Windows) and tar.gz (Linux/macOS) archives
4. Creates a GitHub Release with:
   - Version number
   - All binary downloads
   - Link to CHANGELOG.md
   - Release notes

**Benefits:**
- Automated release process
- Consistent, reproducible builds
- Ready-to-use binaries for end users
- No manual compilation needed

### 4. Dependency Management

**File:** `.github/dependabot.yml`

**Purpose:** Automatically keep dependencies up to date.

**Configuration:**
- NuGet packages: Checked weekly (Mondays at 9 AM)
- GitHub Actions: Checked weekly (Mondays at 9 AM)
- Groups minor and patch updates together
- Maximum 10 open PRs at a time

**Benefits:**
- Security updates applied automatically
- Dependencies stay current
- Reduces maintenance burden

### 5. Issue and PR Templates

**Files:**
- `.github/ISSUE_TEMPLATE/bug_report.md`
- `.github/ISSUE_TEMPLATE/feature_request.md`
- `.github/PULL_REQUEST_TEMPLATE.md`

**Purpose:** Standardize and improve quality of issues and pull requests.

**Benefits:**
- Users provide necessary information
- Consistent format for all issues/PRs
- Faster issue resolution
- Better documentation

### 6. Documentation

**Files:**
- `.github/WORKFLOWS.md` - Detailed workflow documentation
- `.github/README.md` - Quick reference guide
- `CONTRIBUTING.md` - Contributor guidelines
- Updated `README.md` - Added CI/CD badges and release instructions

## How to Use

### For Development (Automatic)

Simply push your code or create a pull request:

```bash
git add .
git commit -m "Add new feature"
git push origin my-branch
```

The CI workflow will automatically:
- Build your code
- Run all tests
- Report results on the PR

### For Releases (Manual Trigger)

When you're ready to release a new version:

1. **Update CHANGELOG.md:**

```markdown
## [1.0.0] - 2025-12-16

### Added
- Initial release with schema export functionality
- Support for Microsoft SQL Server
```

2. **Commit the changes:**

```bash
git add CHANGELOG.md
git commit -m "Release v1.0.0"
git push origin main
```

3. **Create and push a version tag:**

```bash
git tag v1.0.0
git push origin v1.0.0
```

4. **Wait for the workflow to complete** (5-10 minutes)

5. **Download and test the binaries** from the GitHub Releases page

## Monitoring

### View Workflow Runs

1. Go to your repository on GitHub
2. Click the **Actions** tab
3. See all workflow runs and their status

### View Releases

1. Go to your repository on GitHub
2. Click the **Releases** section (right sidebar)
3. Download binaries or view release notes

### Status Badges

Add these badges to your README.md (replace USERNAME/REPO):

```markdown
[![CI](https://github.com/USERNAME/dbdiff/actions/workflows/ci.yml/badge.svg)](https://github.com/USERNAME/dbdiff/actions/workflows/ci.yml)
[![Release](https://github.com/USERNAME/dbdiff/actions/workflows/release.yml/badge.svg)](https://github.com/USERNAME/dbdiff/actions/workflows/release.yml)
```

## Repository Settings

### Required Settings

Before the workflows will work, ensure:

1. **Workflow Permissions:**
   - Go to: Settings → Actions → General
   - Under "Workflow permissions", select "Read and write permissions"
   - Check "Allow GitHub Actions to create and approve pull requests"

2. **Actions Enabled:**
   - Go to: Settings → Actions → General
   - Ensure "Allow all actions and reusable workflows" is selected

### Optional Settings

**Branch Protection (Recommended):**
- Settings → Branches → Add branch protection rule
- Branch name pattern: `main`
- Enable:
  - ✅ Require status checks to pass before merging
  - ✅ Require branches to be up to date before merging
  - Select status checks: `build-and-test`, `multi-platform-test`

This ensures no code is merged without passing tests.

## Files Created

```
.github/
├── workflows/
│   ├── ci.yml                    # Continuous integration
│   ├── pr-check.yml              # Multi-platform PR checks
│   └── release.yml               # Automated releases
├── ISSUE_TEMPLATE/
│   ├── bug_report.md             # Bug report template
│   └── feature_request.md        # Feature request template
├── PULL_REQUEST_TEMPLATE.md      # PR template
├── dependabot.yml                # Dependency updates
├── README.md                     # GitHub config overview
└── WORKFLOWS.md                  # Detailed workflow docs

CONTRIBUTING.md                   # Contribution guidelines
GITHUB-ACTIONS-SETUP.md          # This file
README.md (updated)              # Added CI/CD section and badges
```

## What This Gives You

✅ **Automated Testing** - Every change is automatically tested
✅ **Cross-Platform Validation** - Ensures compatibility across OS
✅ **Automated Releases** - One command creates a full release
✅ **Binary Distribution** - Users can download ready-to-run executables
✅ **Dependency Updates** - Dependabot keeps packages current
✅ **Standardized Process** - Templates ensure quality submissions
✅ **Professional Image** - Shows project maturity and quality

## Next Steps

1. **Push these changes to GitHub:**

```bash
git add .
git commit -m "Add GitHub Actions CI/CD workflows"
git push origin main
```

2. **Configure repository settings** (see above)

3. **Test the CI workflow** by creating a test PR

4. **Create your first release:**

```bash
git tag v0.0.1
git push origin v0.0.1
```

5. **Monitor the Actions tab** to see your workflows in action

6. **Update the README badges** with your actual repository path

## Troubleshooting

### Workflow fails with "Permission denied"

**Solution:** Enable "Read and write permissions" in Settings → Actions → General

### .NET version not found

**Solution:** Ensure .NET 10.0 is available. If not, change to `8.0.x` or `9.0.x` in workflow files

### Release doesn't create

**Solution:** 
1. Check the Actions tab for error details
2. Ensure the tag format is `v1.0.0` (lowercase v, three numbers)
3. Verify workflow permissions are correct

### Dependabot PRs not appearing

**Solution:** 
1. Check Settings → Security & analysis → Dependabot alerts is enabled
2. Wait for the weekly schedule (Mondays at 9 AM)
3. Check Settings → Code security and analysis

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [.NET Build Actions](https://github.com/actions/setup-dotnet)
- [Creating Releases](https://github.com/softprops/action-gh-release)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)

## Support

If you encounter issues:

1. Check the **Actions** tab for detailed logs
2. Review this documentation
3. Check `.github/WORKFLOWS.md` for detailed workflow info
4. Review `CONTRIBUTING.md` for development guidelines
5. Open an issue using the bug report template

---

**Congratulations!** Your project now has a complete CI/CD pipeline. Every push is tested, and creating releases is as simple as pushing a tag. 🎉

