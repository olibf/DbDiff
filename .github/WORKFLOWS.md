# GitHub Actions Workflows

This document describes the automated workflows configured for the DbDiff project.

## Overview

DbDiff uses GitHub Actions for continuous integration, testing, and automated releases. Three workflows are configured:

1. **CI Workflow** - Continuous integration for all pushes
2. **PR Check Workflow** - Multi-platform testing for pull requests
3. **Release Workflow** - Automated release creation with binaries

## Workflows

### 1. CI Workflow (`.github/workflows/ci.yml`)

**Trigger:** Push to `main` or `develop` branches, and pull requests to these branches.

**Purpose:** Ensures code quality by building and testing on every commit.

**Steps:**
1. Checkout code
2. Setup .NET 10.0
3. Restore NuGet dependencies
4. Build in Release configuration
5. Run all tests
6. Upload build artifacts (retained for 7 days)

**Status Badge:**
```markdown
[![CI](https://github.com/olibf/dbdiff/actions/workflows/ci.yml/badge.svg)](https://github.com/olibf/dbdiff/actions/workflows/ci.yml)
```

### 2. PR Check Workflow (`.github/workflows/pr-check.yml`)

**Trigger:** Pull requests to `main` or `develop` branches.

**Purpose:** Validates cross-platform compatibility before merging.

**Platforms Tested:**
- Ubuntu (latest)
- Windows (latest)
- macOS (latest)

**Steps (per platform):**
1. Checkout code
2. Setup .NET 10.0
3. Restore dependencies
4. Build in Release configuration
5. Run all tests
6. Verify publish succeeds for the platform-specific runtime

This ensures the application works correctly on all supported operating systems.

### 3. Release Workflow (`.github/workflows/release.yml`)

**Trigger:** Push of version tags matching `v*.*.*` (e.g., `v1.0.0`, `v1.2.3`).

**Purpose:** Automatically creates releases with platform-specific binaries.

**Platforms Built:**
- Windows x64 (self-contained, single file)
- Linux x64 (self-contained, single file)
- macOS x64 / Intel (self-contained, single file)
- macOS ARM64 / Apple Silicon (self-contained, single file)

**Build Configuration:**
- Release mode
- Self-contained deployment (no .NET installation required)
- Single-file executables
- Version extracted from git tag

**Artifacts Created:**
- `dbdiff-{VERSION}-win-x64.zip`
- `dbdiff-{VERSION}-linux-x64.tar.gz`
- `dbdiff-{VERSION}-osx-x64.tar.gz`
- `dbdiff-{VERSION}-osx-arm64.tar.gz`

**GitHub Release:**
- Automatically creates a GitHub release
- Attaches all binary archives
- Includes download links and references CHANGELOG.md
- Not marked as draft or prerelease

## Creating a Release

To create a new release, follow these steps:

### 1. Update CHANGELOG.md

Update the `CHANGELOG.md` file with your changes following the [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [1.0.0] - 2025-12-16

### Added
- New feature X
- New feature Y

### Changed
- Improved performance of Z

### Fixed
- Bug in component A
```

### 2. Commit Your Changes

```bash
git add CHANGELOG.md
git commit -m "Release v1.0.0"
git push origin main
```

### 3. Create and Push a Tag

Create a version tag following semantic versioning:

```bash
# Create the tag locally
git tag v1.0.0

# Push the tag to GitHub
git push origin v1.0.0
```

### 4. Monitor the Workflow

1. Go to your repository on GitHub
2. Click on the "Actions" tab
3. You should see the "Release" workflow running
4. Wait for the workflow to complete (usually 5-10 minutes)

### 5. Verify the Release

1. Go to the "Releases" section of your repository
2. You should see a new release `v1.0.0`
3. Download links for all platforms should be available
4. Test the binaries on your target platforms

## Version Numbering

This project follows [Semantic Versioning (Semver)](https://semver.org/):

**Format:** `MAJOR.MINOR.PATCH`

- **MAJOR** - Incompatible API changes
- **MINOR** - New functionality (backward compatible)
- **PATCH** - Bug fixes (backward compatible)

**Examples:**
- `v0.0.1` - Initial development
- `v1.0.0` - First stable release
- `v1.1.0` - Added new features (backward compatible)
- `v1.1.1` - Fixed bugs
- `v2.0.0` - Breaking changes

## Permissions

The release workflow requires the following permissions:

- `contents: write` - To create releases and upload assets

These permissions are already configured in the workflow file. Ensure your repository settings allow GitHub Actions to create releases:

1. Go to **Settings** → **Actions** → **General**
2. Under "Workflow permissions", ensure either:
   - "Read and write permissions" is selected, OR
   - "Read repository contents and packages permissions" with "Allow GitHub Actions to create and approve pull requests" enabled

## Troubleshooting

### Workflow Fails with Permission Error

**Problem:** The release workflow fails with a permissions error when trying to create a release.

**Solution:** 
1. Check repository settings: Settings → Actions → General → Workflow permissions
2. Ensure "Read and write permissions" is enabled
3. Re-run the workflow

### .NET Version Not Found

**Problem:** The workflow fails because .NET 10.0 is not available.

**Solution:**
1. Check if .NET 10.0 has been released
2. If using a preview version, update the workflow to use `dotnet-version: '10.0.x'` with `include-prerelease: true`
3. Consider using a stable version like `8.0.x` or `9.0.x` until .NET 10.0 is released

### Build Fails on Specific Platform

**Problem:** The build succeeds on some platforms but fails on others.

**Solution:**
1. Check the workflow logs for the specific platform
2. Look for platform-specific code or dependencies
3. Test locally on that platform if possible
4. Consider adding conditional compilation for platform-specific code

### Release Already Exists

**Problem:** The workflow fails because a release with that tag already exists.

**Solution:**
1. Delete the existing release from GitHub (if incorrect)
2. Delete the tag: `git push --delete origin v1.0.0`
3. Delete local tag: `git tag -d v1.0.0`
4. Create a new tag with the correct version

## Customization

### Changing Target Platforms

To add or remove target platforms, edit `.github/workflows/release.yml`:

```yaml
# Add Windows ARM64
- name: Build and publish for Windows (ARM64)
  run: |
    dotnet publish src/DbDiff.Cli/DbDiff.Cli.csproj \
      -c Release \
      -r win-arm64 \
      --self-contained true \
      -p:PublishSingleFile=true \
      -o ./publish/win-arm64
```

### Changing Build Configuration

To modify build settings, edit the `dotnet publish` commands:

```yaml
# Example: Add trimming to reduce file size
dotnet publish src/DbDiff.Cli/DbDiff.Cli.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ./publish/linux-x64
```

### Adding Additional Steps

You can add additional steps like:
- Code signing
- Checksums generation
- Docker image building
- Publishing to package registries

## Best Practices

1. **Always update CHANGELOG.md** before creating a release
2. **Test locally** before pushing tags
3. **Use semantic versioning** consistently
4. **Keep release notes informative** and user-focused
5. **Monitor workflow runs** to catch issues early
6. **Test downloaded binaries** on actual target platforms

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [.NET Publishing Documentation](https://docs.microsoft.com/en-us/dotnet/core/deploying/)

