param(
    [Parameter(Mandatory=$true, HelpMessage="Destination folder where DbDiff will be installed")]
    [string]$Destination,
    
    [Parameter(Mandatory=$false, HelpMessage="Build configuration (Debug or Release)")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false, HelpMessage="Skip the build and publish step")]
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

# Script variables
$ScriptDir = $PSScriptRoot
$ProjectFile = Join-Path $ScriptDir "src\DbDiff.Cli\DbDiff.Cli.csproj"
$PublishDir = Join-Path $ScriptDir "src\DbDiff.Cli\bin\$Configuration\net10.0\publish"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "DbDiff Installation Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Validate destination path
if ([string]::IsNullOrWhiteSpace($Destination)) {
    Write-Host "Error: Destination path cannot be empty" -ForegroundColor Red
    exit 1
}

# Convert to absolute path
$Destination = [System.IO.Path]::GetFullPath($Destination)
Write-Host "Destination: $Destination" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host ""

# Check if project file exists
if (-not (Test-Path $ProjectFile)) {
    Write-Host "Error: Project file not found at: $ProjectFile" -ForegroundColor Red
    exit 1
}

# Build and publish the project
if (-not $SkipBuild) {
    Write-Host "Building and publishing DbDiff..." -ForegroundColor Green
    Write-Host ""
    
    try {
        dotnet publish $ProjectFile -c $Configuration -o $PublishDir --nologo
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Build/publish failed with exit code $LASTEXITCODE" -ForegroundColor Red
            exit $LASTEXITCODE
        }
        
        Write-Host ""
        Write-Host "Build completed successfully!" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host "Error during build: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "Skipping build (using existing publish folder)" -ForegroundColor Yellow
    Write-Host ""
}

# Check if publish directory exists
if (-not (Test-Path $PublishDir)) {
    Write-Host "Error: Publish directory not found at: $PublishDir" -ForegroundColor Red
    Write-Host "Run without -SkipBuild to build the project first" -ForegroundColor Yellow
    exit 1
}

# Get file count in publish directory
$FilesToCopy = Get-ChildItem -Path $PublishDir -Recurse -File
$FileCount = $FilesToCopy.Count

if ($FileCount -eq 0) {
    Write-Host "Error: No files found in publish directory" -ForegroundColor Red
    exit 1
}

Write-Host "Found $FileCount files to install" -ForegroundColor Cyan
Write-Host ""

# Create destination directory if it doesn't exist, or clear it if it does
if (Test-Path $Destination) {
    Write-Host "Clearing existing destination folder..." -ForegroundColor Yellow
    
    # Confirm before deleting if destination is not empty
    $ExistingFiles = Get-ChildItem -Path $Destination -Recurse -File -ErrorAction SilentlyContinue
    if ($ExistingFiles) {
        $ExistingCount = $ExistingFiles.Count
        Write-Host "  Warning: $ExistingCount existing files will be deleted" -ForegroundColor Yellow
        
        # Safety check: don't allow deleting critical system folders
        $CriticalPaths = @(
            $env:SystemRoot,
            $env:ProgramFiles,
            ${env:ProgramFiles(x86)},
            $env:USERPROFILE,
            "$env:SystemDrive\"
        )
        
        foreach ($criticalPath in $CriticalPaths) {
            if ($Destination -eq $criticalPath -or $Destination.StartsWith("$criticalPath\", [StringComparison]::OrdinalIgnoreCase)) {
                Write-Host "Error: Cannot install to system folder: $Destination" -ForegroundColor Red
                exit 1
            }
        }
        
        try {
            # Remove all files and subdirectories
            Get-ChildItem -Path $Destination -Recurse | Remove-Item -Force -Recurse -ErrorAction Stop
            Write-Host "  Destination cleared successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "Error clearing destination: $_" -ForegroundColor Red
            exit 1
        }
    }
}
else {
    Write-Host "Creating destination folder..." -ForegroundColor Yellow
    try {
        New-Item -Path $Destination -ItemType Directory -Force | Out-Null
        Write-Host "  Destination created successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Error creating destination: $_" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Installing DbDiff..." -ForegroundColor Green

# Copy files from publish directory to destination
try {
    Copy-Item -Path "$PublishDir\*" -Destination $Destination -Recurse -Force
    Write-Host "  $FileCount files copied successfully" -ForegroundColor Green
}
catch {
    Write-Host "Error copying files: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Installation completed!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "DbDiff is now installed at:" -ForegroundColor White
Write-Host "  $Destination" -ForegroundColor Yellow
Write-Host ""
Write-Host "To run DbDiff:" -ForegroundColor White
Write-Host "  $Destination\DbDiff.Cli.exe --help" -ForegroundColor Yellow
Write-Host ""

# Optionally add to PATH
$AddToPath = Read-Host "Would you like to add this location to your PATH? (y/n)"
if ($AddToPath -eq 'y' -or $AddToPath -eq 'Y') {
    try {
        $CurrentPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User)
        
        if ($CurrentPath -notlike "*$Destination*") {
            $NewPath = "$CurrentPath;$Destination"
            [Environment]::SetEnvironmentVariable("Path", $NewPath, [EnvironmentVariableTarget]::User)
            Write-Host "Added to PATH successfully!" -ForegroundColor Green
            Write-Host "Note: Restart your terminal for PATH changes to take effect" -ForegroundColor Yellow
        }
        else {
            Write-Host "Location already in PATH" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Warning: Could not add to PATH: $_" -ForegroundColor Yellow
        Write-Host "You can manually add it to your PATH environment variable" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green

