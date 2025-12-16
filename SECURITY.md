# Security

## Overview

DbDiff takes security seriously. This document outlines the security measures implemented in the application and provides guidance for secure usage.

## Security Features

### 1. Path Validation and Sanitization

**Implemented (unreleased)**

All file paths (output files, configuration files, and log files) are validated and sanitized to prevent path traversal attacks and unauthorized file system access.

#### Output Path Validation

- **Location**: `PathValidator.ValidateOutputPath()`
- **Protection Against**:
  - Path traversal attacks (e.g., `../../etc/passwd`)
  - Writing to system directories (Windows: `C:\Windows`, `C:\Program Files`, etc.; Unix: `/etc`, `/bin`, `/sys`, etc.)
  - Invalid file names and characters
  - Unauthorized directory access

#### Configuration Path Validation

- **Location**: `PathValidator.ValidateConfigPath()`
- **Protection Against**:
  - Reading arbitrary files outside allowed directories
  - Path traversal attacks
  - Non-JSON file injection
  - Missing or inaccessible files

#### Log Path Validation

- **Location**: `PathValidator.ValidateLogPath()`
- **Protection Against**:
  - Writing logs to system directories
  - Path traversal in log configuration

### 2. SQL Injection Prevention

- All database queries use **parameterized queries** (prepared statements)
- No dynamic SQL construction from user input
- Schema and table names are retrieved from database metadata and parameterized

**Example**:
```csharp
columnCommand.Parameters.AddWithValue("@SchemaName", schemaName);
columnCommand.Parameters.AddWithValue("@TableName", tableName);
```

### 3. Input Validation

- Connection strings are validated for null/empty values
- All DTOs validate their inputs in constructors
- Argument null/empty checks throughout the codebase

## Security Best Practices for Users

### Connection Strings

1. **Never hardcode credentials** in configuration files
2. **Use Windows Authentication** when possible:
   ```
   Server=localhost;Database=MyDb;Trusted_Connection=true;
   ```

3. **Use environment variables** for sensitive connection strings:
   ```bash
   # Set environment variable
   $env:DBDIFF_ConnectionStrings__Default = "Server=localhost;Database=MyDb;..."
   
   # Run without exposing credentials in command line
   dbdiff -o output.txt
   ```

4. **Enable encryption** in production:
   ```
   Server=prod-server;Database=MyDb;User Id=dbuser;Password=***;Encrypt=true;TrustServerCertificate=false;
   ```

### File Paths

1. **Use relative paths** within your working directory when possible
2. **Avoid absolute paths** to system directories
3. **Be aware** that path validation restricts writes to:
   - Windows: `C:\Windows`, `C:\Program Files`, `C:\ProgramData`
   - Unix/Linux: `/etc`, `/bin`, `/sbin`, `/usr/bin`, `/usr/sbin`, `/sys`, `/proc`, `/boot`, `/root`

### Configuration Files

1. **Store configuration files** in your project directory
2. **Use `.gitignore`** to exclude files containing sensitive information:
   ```gitignore
   appsettings.*.json
   *.local.json
   ```

3. **Use file permissions** to protect configuration files:
   ```bash
   # Unix/Linux
   chmod 600 appsettings.json
   ```

### Logging

1. **Review log files** regularly for suspicious activity
2. **Protect log directories** with appropriate file permissions
3. **Rotate logs** regularly (DbDiff uses Serilog with 7-day retention by default)

## Reporting Security Vulnerabilities

If you discover a security vulnerability in DbDiff, please report it responsibly:

1. **Do not** create a public GitHub issue
2. Contact the maintainers directly via email or private message
3. Provide detailed information about the vulnerability
4. Allow time for a fix to be developed and released

## Security Audit History

| Date | Version | Changes |
|------|---------|---------|
| 2025-12-16 | Unreleased | Implemented comprehensive path validation for output, config, and log files |
| 2025-12-16 | Unreleased | Initial development with parameterized SQL queries |

## Known Limitations

1. **No connection string encryption**: Connection strings in configuration files are stored in plain text. Use environment variables or Windows DPAPI for sensitive environments.

2. **No built-in encryption enforcement**: The application does not enforce `Encrypt=true` in SQL Server connection strings. Users must configure this manually for production environments.

3. **File permission checks**: Path validation checks logical paths but relies on the operating system for actual file permission enforcement.

## Planned Security Enhancements

- [ ] Connection string validation and encryption enforcement
- [ ] Support for Azure Key Vault and other secret stores
- [ ] Connection string sanitization in logs
- [ ] Rate limiting for database operations
- [ ] Audit logging for all file system operations

## Dependencies

DbDiff relies on the following security-related packages:

- **Microsoft.Data.SqlClient** (6.1.3): Official SQL Server client with built-in protection against SQL injection
- **Serilog** (10.0.0): Structured logging framework with safe parameter handling
- **.NET 10.0**: Latest .NET runtime with security improvements

Keep dependencies up to date to receive security patches.

## Compliance

### OWASP Top 10 Coverage

- ✅ **A03:2021 – Injection**: Protected via parameterized queries
- ✅ **A01:2021 – Broken Access Control**: Path validation prevents unauthorized file access
- ✅ **A04:2021 – Insecure Design**: Security-first design with validation layers
- ⚠️ **A07:2021 – Identification and Authentication Failures**: Relies on database authentication
- ⚠️ **A02:2021 – Cryptographic Failures**: Connection strings not encrypted at rest

## Testing

Security measures are validated through comprehensive unit tests:

- **23 path validation tests** covering:
  - Valid and invalid paths
  - Path traversal attempts
  - System directory protection
  - Base path restrictions
  - Invalid characters and formats

Run security tests:
```bash
dotnet test src/DbDiff.Application.Tests/DbDiff.Application.Tests.csproj
```

## Additional Resources

- [OWASP Secure Coding Practices](https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [SQL Server Connection Security](https://docs.microsoft.com/en-us/sql/connect/ado-net/sql/sqlclient-support-alwayson)

