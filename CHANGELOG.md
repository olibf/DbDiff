# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **PostgreSQL database support** - Full schema extraction for PostgreSQL databases
- `PostgreSqlSchemaExtractor` implementing `ISchemaExtractor` for PostgreSQL connectivity
- `SchemaExtractorFactory` to dynamically instantiate the correct extractor based on database type
- `DatabaseType` enum in Domain layer (`SqlServer`, `PostgreSql`)
- `--database-type` (`-d`) CLI parameter to explicitly specify database type
- Automatic database type detection from connection string patterns
- Cross-database schema comparison capability (compare SQL Server and PostgreSQL schemas)
- Npgsql package dependency for PostgreSQL connectivity
- View extraction and export functionality for SQL Server
- View SQL definitions included in exports (using sys.sql_modules for complete definitions)
- View column structures with full metadata (data types, nullability, precision, etc.)
- Alphabetically sorted views in output for deterministic comparison
- Support for encrypted views (gracefully handles NULL definitions)
- `--exclude-view-definitions` CLI flag to optionally exclude SQL definitions from output

### Changed
- Updated `SchemaExportRequest` DTO to include `DatabaseType` property
- Refactored `SchemaExportService` to use factory pattern for extractor instantiation
- Updated CLI help text with PostgreSQL examples and database type parameter
- Updated `DatabaseSchema` domain entity to include `Views` collection
- Enhanced `CustomTextFormatter` to display view definitions and columns
- Improved logging to report both table and view counts
- Updated `SchemaExportResult` DTO with separate `TableCount` and `ViewCount` properties
- Made view definitions optional in export output via `IncludeViewDefinitions` formatter property

## [0.0.1] - 2025-12-16

### Added
- Comprehensive path validation and sanitization to prevent path traversal attacks
- `PathValidator` utility class in Application layer with three validation methods:
  - `ValidateOutputPath()` - Validates and sanitizes output file paths
  - `ValidateConfigPath()` - Validates configuration file paths with JSON enforcement
  - `ValidateLogPath()` - Validates log file paths
- Protection against writing to system directories (Windows: C:\Windows, Program Files; Unix: /etc, /bin, etc.)
- Unit tests for path validation (23 tests covering various security scenarios)
- SECURITY.md documentation detailing security features and best practices
- MSSQL database schema extraction functionality
- Support for exporting table schemas with column information (name, data type, nullability, max length, precision, scale)
- Column ordinal position tracking with `--ignore-position` flag to exclude from output
- Custom text format output optimized for diff comparison
- CLI with command-line argument support
- Configuration via appsettings.json, environment variables, and CLI arguments
- Serilog logging to file
- Hexagonal architecture (Ports & Adapters) implementation
- PowerShell installation and uninstallation scripts for Windows
- MIT License - open source release

### Security
- **CRITICAL FIX**: Path traversal vulnerability in output file paths
- **CRITICAL FIX**: Arbitrary file reading via --config parameter
- **CRITICAL FIX**: Path traversal in log configuration
- All file paths now validated before use
- Restricted file system access to safe directories
- SQL injection protection via parameterized queries
- Input validation for connection strings and paths


