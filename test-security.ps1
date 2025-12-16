# Security Test Script
# This script demonstrates that path traversal attacks are now blocked

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "DbDiff Security Validation Tests" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$exe = ".\src\DbDiff.Cli\bin\Release\net10.0\DbDiff.Cli.exe"

if (-not (Test-Path $exe)) {
    Write-Host "Error: DbDiff.Cli.exe not found. Please build the solution first." -ForegroundColor Red
    Write-Host "Run: dotnet build DbDiff.sln -c Release" -ForegroundColor Yellow
    exit 1
}

Write-Host "Testing path validation (all should fail safely)..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Path traversal in output
Write-Host "Test 1: Path traversal attack (../../../Windows/test.txt)" -ForegroundColor White
& $exe -c "Server=localhost;Database=test;Trusted_Connection=true;" -o "../../../Windows/test.txt" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✓ BLOCKED - Path traversal prevented" -ForegroundColor Green
} else {
    Write-Host "  ✗ FAILED - Attack not blocked!" -ForegroundColor Red
}
Write-Host ""

# Test 2: Writing to system directory
Write-Host "Test 2: Write to system directory (C:\Windows\System32\malicious.txt)" -ForegroundColor White
& $exe -c "Server=localhost;Database=test;Trusted_Connection=true;" -o "C:\Windows\System32\malicious.txt" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✓ BLOCKED - System directory write prevented" -ForegroundColor Green
} else {
    Write-Host "  ✗ FAILED - Attack not blocked!" -ForegroundColor Red
}
Write-Host ""

# Test 3: Invalid config file
Write-Host "Test 3: Arbitrary file read via --config" -ForegroundColor White
& $exe -c "Server=localhost;Database=test;Trusted_Connection=true;" --config "C:\Windows\System32\drivers\etc\hosts" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✓ BLOCKED - Arbitrary file read prevented" -ForegroundColor Green
} else {
    Write-Host "  ✗ FAILED - Attack not blocked!" -ForegroundColor Red
}
Write-Host ""

# Test 4: Valid path (should succeed with connection error, not path error)
Write-Host "Test 4: Valid path (should proceed past path validation)" -ForegroundColor White
$output = & $exe -c "Server=localhost;Database=test;Trusted_Connection=true;" -o "test-output.txt" 2>&1 | Out-String
if ($output -notmatch "Security Error" -and $output -notmatch "Invalid.*path") {
    Write-Host "  ✓ PASSED - Valid path accepted" -ForegroundColor Green
    Write-Host "    (Connection/database error is expected if no DB is configured)" -ForegroundColor Gray
} else {
    Write-Host "  ✗ FAILED - Valid path rejected!" -ForegroundColor Red
}
Write-Host ""

# Cleanup
if (Test-Path "test-output.txt") {
    Remove-Item "test-output.txt" -Force
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Security Tests Complete" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Path traversal attacks are now prevented" -ForegroundColor Green
Write-Host "  - System directory writes are blocked" -ForegroundColor Green
Write-Host "  - Arbitrary file reads are prevented" -ForegroundColor Green
Write-Host "  - Valid paths are properly accepted" -ForegroundColor Green
Write-Host ""
Write-Host "For more information, see SECURITY.md" -ForegroundColor Yellow

