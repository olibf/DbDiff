# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
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


