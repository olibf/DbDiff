# GitHub Actions Quick Start Guide

Get your CI/CD pipeline up and running in 5 minutes!

## Step 1: Push to GitHub (2 minutes)

If this is a new repository, create it on GitHub first, then:

```bash
# Initialize git (if not already done)
git init

# Add all files
git add .

# Commit
git commit -m "Add GitHub Actions CI/CD workflows"

# Add remote (replace with your repository URL)
git remote add origin https://github.com/YOUR-USERNAME/dbdiff.git

# Push to GitHub
git push -u origin main
```

If you already have a repository:

```bash
# Add and commit the new files
git add .github/ CONTRIBUTING.md GITHUB-ACTIONS-SETUP.md README.md
git commit -m "Add GitHub Actions CI/CD workflows"

# Push
git push origin main
```

## Step 2: Configure Repository Settings (2 minutes)

### Enable Workflow Permissions

1. Go to your repository on GitHub
2. Click **Settings** (top right)
3. Click **Actions** → **General** (left sidebar)
4. Scroll to "Workflow permissions"
5. Select **"Read and write permissions"**
6. Check **"Allow GitHub Actions to create and approve pull requests"**
7. Click **Save**

### Verify Actions are Enabled

While in Settings → Actions → General:
- Under "Actions permissions", ensure **"Allow all actions and reusable workflows"** is selected
- Click **Save** if you made changes

## Step 3: Test the CI Workflow (1 minute)

The CI workflow should have run automatically when you pushed to `main`. Let's verify:

1. Go to the **Actions** tab in your repository
2. You should see a workflow run for your recent push
3. Click on it to see the details
4. Watch the build and test steps execute

**If you see a green checkmark ✅** - Success! Your CI is working!

**If you see a red X ❌** - Click on the failed step to see error details

## Step 4: Create Your First Release (Optional)

Ready to create a release? Let's make version 0.0.1:

### Update CHANGELOG.md

Edit `CHANGELOG.md` and add your release:

```markdown
## [0.0.1] - 2025-12-16

### Added
- Initial release
- MSSQL schema export functionality
- CLI interface
- GitHub Actions CI/CD pipeline
```

### Commit and Tag

```bash
# Commit the changelog
git add CHANGELOG.md
git commit -m "Release v0.0.1"
git push origin main

# Create a version tag
git tag v0.0.1

# Push the tag
git push origin v0.0.1
```

### Wait and Download

1. Go to the **Actions** tab
2. Watch the "Release" workflow run (takes 5-10 minutes)
3. When complete, go to the **Releases** section
4. You'll see your release with binaries for Windows, Linux, and macOS!
5. Download and test them

## Step 5: Add Status Badges (Optional)

Update your `README.md` to show your workflow status:

Replace the placeholders in the README.md badges section with your actual GitHub username/organization and repository name:

```markdown
[![CI](https://github.com/YOUR-USERNAME/dbdiff/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR-USERNAME/dbdiff/actions/workflows/ci.yml)
[![Release](https://github.com/YOUR-USERNAME/dbdiff/actions/workflows/release.yml/badge.svg)](https://github.com/YOUR-USERNAME/dbdiff/actions/workflows/release.yml)
```

For example, if your GitHub username is `johndoe`:

```markdown
[![CI](https://github.com/johndoe/dbdiff/actions/workflows/ci.yml/badge.svg)](https://github.com/johndoe/dbdiff/actions/workflows/ci.yml)
[![Release](https://github.com/johndoe/dbdiff/actions/workflows/release.yml/badge.svg)](https://github.com/johndoe/dbdiff/actions/workflows/release.yml)
```

## What Happens Next?

### Automatically

✅ **Every push** triggers the CI workflow (build + test)  
✅ **Every PR** triggers multi-platform testing  
✅ **Every Monday** Dependabot checks for dependency updates  
✅ **Every tag push** (v*.*.*) creates a release with binaries

### Manually

You can:
- View workflow runs in the **Actions** tab
- Download release binaries from **Releases**
- Review and merge Dependabot PRs
- Monitor build status with badges

## Troubleshooting

### "Permission denied" error in workflow

**Fix:** Go to Settings → Actions → General → Workflow permissions → Select "Read and write permissions"

### .NET version error

**Fix:** Edit `.github/workflows/*.yml` files and change `dotnet-version: '10.0.x'` to `'8.0.x'` or `'9.0.x'`

### Release workflow didn't run

**Fix:** 
- Ensure tag format is `v1.0.0` (lowercase v)
- Check the Actions tab for errors
- Verify workflow permissions are correct

### No build artifacts in release

**Fix:**
- Check the workflow log for build errors
- Ensure `src/DbDiff.Cli/DbDiff.Cli.csproj` path is correct
- Verify the project builds locally: `dotnet publish -c Release`

## Need More Help?

- 📖 Read [WORKFLOWS.md](.github/WORKFLOWS.md) for detailed workflow documentation
- 📋 Read [GITHUB-ACTIONS-SETUP.md](../GITHUB-ACTIONS-SETUP.md) for the complete setup summary
- 🤝 Read [CONTRIBUTING.md](../CONTRIBUTING.md) for development guidelines
- ❓ Open an issue using the bug report template

## Congratulations! 🎉

Your project now has:
- ✅ Automated testing on every commit
- ✅ Cross-platform validation
- ✅ One-command releases
- ✅ Automated dependency updates
- ✅ Professional project structure

You can now focus on building features while GitHub Actions handles the boring stuff!

