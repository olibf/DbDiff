param(
    [Parameter(Mandatory=$true, HelpMessage="Installation folder to remove")]
    [string]$InstallPath
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "DbDiff Uninstall Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Validate and convert to absolute path
if ([string]::IsNullOrWhiteSpace($InstallPath)) {
    Write-Host "Error: Installation path cannot be empty" -ForegroundColor Red
    exit 1
}

$InstallPath = [System.IO.Path]::GetFullPath($InstallPath)

# Check if path exists
if (-not (Test-Path $InstallPath)) {
    Write-Host "Warning: Installation path does not exist: $InstallPath" -ForegroundColor Yellow
    Write-Host "Nothing to uninstall" -ForegroundColor Yellow
    exit 0
}

# Safety check: verify it looks like a DbDiff installation
$DbDiffExe = Join-Path $InstallPath "DbDiff.Cli.exe"
if (-not (Test-Path $DbDiffExe)) {
    Write-Host "Warning: DbDiff.Cli.exe not found in: $InstallPath" -ForegroundColor Yellow
    $Confirm = Read-Host "This doesn't appear to be a DbDiff installation. Continue anyway? (y/n)"
    if ($Confirm -ne 'y' -and $Confirm -ne 'Y') {
        Write-Host "Uninstall cancelled" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Installation path: $InstallPath" -ForegroundColor Yellow
Write-Host ""

# Confirm uninstallation
Write-Host "WARNING: This will delete the entire folder and all its contents!" -ForegroundColor Red
$Confirm = Read-Host "Are you sure you want to uninstall DbDiff from this location? (y/n)"

if ($Confirm -ne 'y' -and $Confirm -ne 'Y') {
    Write-Host "Uninstall cancelled" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Uninstalling DbDiff..." -ForegroundColor Yellow

# Remove from PATH if present
try {
    $CurrentPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User)
    
    if ($CurrentPath -like "*$InstallPath*") {
        Write-Host "  Removing from PATH..." -ForegroundColor Yellow
        
        # Split PATH, remove the installation path, and rejoin
        $PathArray = $CurrentPath -split ';' | Where-Object { $_ -ne $InstallPath -and $_ -ne "$InstallPath\" }
        $NewPath = $PathArray -join ';'
        
        [Environment]::SetEnvironmentVariable("Path", $NewPath, [EnvironmentVariableTarget]::User)
        Write-Host "  Removed from PATH" -ForegroundColor Green
    }
}
catch {
    Write-Host "  Warning: Could not remove from PATH: $_" -ForegroundColor Yellow
}

# Delete installation folder
try {
    Remove-Item -Path $InstallPath -Recurse -Force -ErrorAction Stop
    Write-Host "  Installation folder deleted" -ForegroundColor Green
}
catch {
    Write-Host "Error deleting installation folder: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Uninstall completed!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "DbDiff has been removed from: $InstallPath" -ForegroundColor White
Write-Host ""
Write-Host "Note: If PATH was modified, restart your terminal for changes to take effect" -ForegroundColor Yellow
Write-Host ""

