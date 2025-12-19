# DbDiff - Database Schema Comparison Tool

A CLI tool for exporting and comparing database schemas, built with .NET following hexagonal architecture principles.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CI](https://github.com/olibf/dbdiff/actions/workflows/ci.yml/badge.svg)](https://github.com/olibf/dbdiff/actions/workflows/ci.yml)
[![Release](https://github.com/olibf/dbdiff/actions/workflows/release.yml/badge.svg)](https://github.com/olibf/dbdiff/actions/workflows/release.yml)

## Version

Current version: **0.0.1**

## Features

- Export database schemas to text format from SQL Server and PostgreSQL
- **Complete schema export including:**
  - Tables with full column definitions
  - Views with SQL definitions and column structures
- Deterministic, diff-friendly output format
- Alphabetically sorted tables and columns for easy comparison
- Cross-database schema comparison (compare SQL Server vs PostgreSQL schemas)
- Automatic database type detection from connection strings
- Configurable via CLI arguments, environment variables, or configuration files
- Structured logging with Serilog
- **Comprehensive security features:**
  - Path validation and sanitization to prevent path traversal attacks
  - Protection against writing to system directories
  - SQL injection prevention via parameterized queries
  - Input validation throughout

## Installation

### Quick Install (Windows)

Use the provided PowerShell installation script:

```powershell
.\install.ps1 -Destination "C:\Tools\DbDiff"
```

This will:
1. Build and publish the project in Release mode
2. Clear the destination folder (if it exists)
3. Copy all files to the destination
4. Optionally add the destination to your PATH

**Script Parameters:**
- `-Destination` (required): Installation folder path
- `-Configuration`: Build configuration (`Release` or `Debug`, default: `Release`)
- `-SkipBuild`: Skip building and use existing publish folder

**Examples:**
```powershell
# Install to a custom location
.\install.ps1 -Destination "C:\Tools\DbDiff"

# Install Debug build
.\install.ps1 -Destination "C:\Tools\DbDiff" -Configuration Debug

# Install without rebuilding (use existing publish folder)
.\install.ps1 -Destination "C:\Tools\DbDiff" -SkipBuild
```

### Uninstall (Windows)

Use the uninstall script to remove DbDiff:

```powershell
.\uninstall.ps1 -InstallPath "C:\Tools\DbDiff"
```

This will:
1. Remove the installation folder and all its contents
2. Remove the path from your PATH environment variable (if present)

### Manual Build from Source

```bash
dotnet build
```

### Run Without Installing

```bash
cd src/DbDiff.Cli
dotnet run -- --connection "Your-Connection-String" --output schema.txt
```

Or build and run the published executable:

```bash
dotnet publish -c Release
cd src/DbDiff.Cli/bin/Release/net10.0/publish
./DbDiff.Cli --connection "Your-Connection-String"
```

## Usage

### Basic Usage

**SQL Server:**
```bash
dbdiff --connection "Server=localhost;Database=MyDb;Trusted_Connection=true;" --output schema.txt
```

**PostgreSQL:**
```bash
dbdiff --connection "Host=localhost;Database=mydb;Username=user;Password=pass" --output schema.txt
```

### Command-Line Options

- `-c, --connection <string>`: Database connection string (required)
- `-o, --output <path>`: Output file path (default: schema.txt)
- `-d, --database-type <type>`: Database type: `sqlserver`, `postgresql` (auto-detected if not specified)
- `--config <path>`: Path to configuration file (default: appsettings.json)
- `--ignore-position`: Exclude column ordinal positions from output
- `--exclude-view-definitions`: Exclude view SQL definitions from output (column structure still included)
- `-h, --help`: Show help information

### Database Type Detection

DbDiff can automatically detect the database type from your connection string:
- **SQL Server** keywords: `Server=`, `Data Source=`, `Initial Catalog=`, `Integrated Security=`
- **PostgreSQL** keywords: `Host=`, `Username=`

You can override auto-detection using the `--database-type` parameter.

### Configuration

Connection strings and output paths can be configured in multiple ways (priority order):

1. **Command-line arguments** (highest priority)
2. **Environment variables**
   - `DBDIFF_ConnectionStrings__Default`
   - `DBDIFF_Export__OutputPath`
3. **Configuration file** (`appsettings.json`)

#### Example appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=MyDb;Trusted_Connection=true;"
  },
  "Export": {
    "OutputPath": "schema.txt"
  },
  "Serilog": {
    "LogPath": "logs/dbdiff-.txt",
    "RollingInterval": "Day",
    "RetainedFileCountLimit": 7
  }
}
```

## Output Format

The tool exports database schemas in a custom text format optimized for diff tools:

```
DATABASE: MyDatabase
EXTRACTED: 2025-12-16T12:00:00.000Z

TABLE: dbo.Users
  COLUMN: Email
    OrdinalPosition: 3
    Type: nvarchar
    Nullable: No
    MaxLength: 255
  COLUMN: Id
    OrdinalPosition: 1
    Type: int
    Nullable: No
  COLUMN: Name
    OrdinalPosition: 2
    Type: nvarchar
    Nullable: Yes
    MaxLength: 100

VIEW: dbo.ActiveUsers
  DEFINITION:
    CREATE VIEW dbo.ActiveUsers AS
    SELECT Id, Name, Email
    FROM dbo.Users
    WHERE IsActive = 1
  COLUMN: Email
    OrdinalPosition: 3
    Type: nvarchar
    Nullable: Yes
    MaxLength: 255
  COLUMN: Id
    OrdinalPosition: 1
    Type: int
    Nullable: No
  COLUMN: Name
    OrdinalPosition: 2
    Type: nvarchar
    Nullable: Yes
    MaxLength: 100
```

Features:
- Tables, views, and columns are alphabetically sorted
- Views include complete SQL definitions for comparison
- Consistent formatting and indentation
- One property per line
- Deterministic output for reliable diffing

## Architecture

The project follows **Hexagonal Architecture** (Ports & Adapters) with clear separation of concerns:

```
┌─────────────┐
│  DbDiff.Cli │ ──► Entry point, CLI argument parsing
└──────┬──────┘
       │
┌──────▼───────────────┐
│ DbDiff.Application   │ ──► Use cases, DTOs, Formatters
└──────┬───────────────┘
       │
┌──────▼────────────┐        ┌──────────────────────┐
│  DbDiff.Domain    │ ◄───── │ DbDiff.Infrastructure│
│  (Port Interfaces)│        │  (Adapters: MSSQL)   │
└───────────────────┘        └──────────────────────┘
```

### Project Structure

- **DbDiff.Domain**: Core domain entities and port interfaces (no dependencies)
- **DbDiff.Application**: Application services, DTOs, and formatters
- **DbDiff.Infrastructure**: Database-specific implementations (MSSQL)
- **DbDiff.Cli**: Console application entry point

## Supported Databases

- ✅ Microsoft SQL Server (MSSQL)
- ✅ PostgreSQL

### Cross-Database Schema Comparison

DbDiff uses a consistent output format for all database types, making it possible to compare schemas across different database platforms:

```bash
# Export SQL Server schema
dbdiff --connection "Server=localhost;Database=MyDb;Trusted_Connection=true;" --output sqlserver-schema.txt

# Export PostgreSQL schema
dbdiff --connection "Host=localhost;Database=mydb;Username=user;Password=pass" --output postgres-schema.txt

# Compare using your favorite diff tool
diff sqlserver-schema.txt postgres-schema.txt
```

**Note:** Data types will differ between platforms (e.g., SQL Server's `nvarchar` vs PostgreSQL's `character varying`), but the consistent format allows you to easily identify structural differences.

## Development

### Prerequisites

- .NET 10.0 SDK or later
- SQL Server (for SQL Server support)
- PostgreSQL (for PostgreSQL support)

### Dependencies

- **Microsoft.Data.SqlClient**: SQL Server connectivity
- **Npgsql**: PostgreSQL connectivity
- **Serilog**: Structured logging
- **Microsoft.Extensions.***: Configuration and dependency injection

### Running Tests

```bash
dotnet test
```

### CI/CD with GitHub Actions

This project includes automated workflows for continuous integration and releases:

#### Continuous Integration (CI)

The CI workflow runs automatically on every push or pull request to `main` or `develop` branches:
- Restores dependencies
- Builds the project in Release mode
- Runs all tests
- Uploads build artifacts

#### Pull Request Checks

Pull requests trigger multi-platform testing on:
- Ubuntu (Linux)
- Windows
- macOS

This ensures compatibility across all supported platforms.

#### Automated Releases

To create a release:

1. Update the version in `CHANGELOG.md` following semantic versioning
2. Commit your changes
3. Create and push a version tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The release workflow automatically:
- Builds self-contained executables for:
  - Windows x64
  - Linux x64
  - macOS x64 (Intel)
  - macOS ARM64 (Apple Silicon)
- Creates release archives (ZIP for Windows, tar.gz for Linux/macOS)
- Creates a GitHub release with all binaries attached
- Links to the CHANGELOG for release notes

**Note:** GitHub Actions requires appropriate permissions. Ensure your repository settings allow Actions to create releases.

## Logging

Logs are written to the `logs/` directory by default. The log file path and retention policy can be configured in `appsettings.json`:

```json
{
  "Serilog": {
    "LogPath": "logs/dbdiff-.txt",
    "RollingInterval": "Day",
    "RetainedFileCountLimit": 7
  }
}
```

## Roadmap

See [CHANGELOG.md](CHANGELOG.md) for version history and planned features.

### Future Enhancements

- Schema comparison (diff) functionality
- Support for additional database objects (indexes, foreign keys, stored procedures, functions, triggers, etc.)
- GUI application using AvaloniaUI
- Multiple output formats (JSON, YAML, SQL DDL)
- Support for additional databases (MySQL, Oracle, etc.)

## Security

DbDiff takes security seriously. We've implemented comprehensive security measures including:

- **Path validation**: All file paths are validated and sanitized to prevent path traversal attacks
- **SQL injection prevention**: All database queries use parameterized statements
- **Input validation**: Comprehensive validation of all user inputs

For detailed security information, please see [SECURITY.md](SECURITY.md).

### Reporting Security Vulnerabilities

If you discover a security vulnerability, please report it responsibly by contacting the maintainers directly rather than creating a public issue.

## Contributing

Contributions are welcome! Please follow the existing code structure and architectural principles.

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

### MIT License Summary

- ✅ Commercial use allowed
- ✅ Modification allowed
- ✅ Distribution allowed
- ✅ Private use allowed
- ❗ License and copyright notice must be included

