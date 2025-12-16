# Contributing to DbDiff

Thank you for your interest in contributing to DbDiff! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Release Process](#release-process)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for everyone.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Git
- A code editor (Visual Studio, Visual Studio Code, Rider, etc.)
- SQL Server or PostgreSQL for testing

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:

```bash
git clone https://github.com/YOUR-USERNAME/dbdiff.git
cd dbdiff
```

3. Add the upstream repository:

```bash
git remote add upstream https://github.com/ORIGINAL-OWNER/dbdiff.git
```

### Build the Project

```bash
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test
```

## Development Workflow

1. **Create a branch** for your work:

```bash
git checkout -b feature/my-new-feature
# or
git checkout -b fix/bug-description
```

2. **Make your changes** following the coding standards
3. **Write or update tests** for your changes
4. **Run tests** to ensure everything works
5. **Commit your changes** with clear, descriptive messages
6. **Push to your fork**
7. **Open a Pull Request** against the `develop` branch

## Coding Standards

### Architecture

DbDiff follows **Hexagonal Architecture (Ports & Adapters)** with strict separation of concerns:

- **Domain Layer** (`DbDiff.Domain`)
  - Core business entities
  - Port interfaces (e.g., `ISchemaExtractor`)
  - No external dependencies
  - Pure C# classes

- **Application Layer** (`DbDiff.Application`)
  - Use cases and application services
  - DTOs for data transfer
  - Formatters and validators
  - Depends only on Domain layer

- **Infrastructure Layer** (`DbDiff.Infrastructure`)
  - Database-specific implementations
  - Adapters implementing domain ports
  - External library integrations
  - Depends on Domain layer

- **CLI Layer** (`DbDiff.Cli`)
  - Entry point and CLI argument parsing
  - Dependency injection configuration
  - Depends on all other layers

### C# Guidelines

1. **Namespaces**: Omit namespace declarations where possible (use file-scoped or implicit)

```csharp
// Good
namespace DbDiff.Domain;

public class Table
{
    // ...
}
```

2. **Top-level programs**: Use classless top-level programs for entry points

```csharp
// Good - Program.cs
using DbDiff.Application;

var builder = WebApplication.CreateBuilder(args);
// ...
```

3. **SOLID Principles**: Follow SOLID principles throughout

4. **Async/Await**: Use async/await for all I/O operations

5. **Dependency Injection**: Use constructor injection for dependencies

```csharp
public class SchemaExportService
{
    private readonly ISchemaExtractor _extractor;
    private readonly ISchemaFormatter _formatter;

    public SchemaExportService(ISchemaExtractor extractor, ISchemaFormatter formatter)
    {
        _extractor = extractor;
        _formatter = formatter;
    }
}
```

6. **Mapping**: Always map between layers (no direct exposure of domain entities)

```csharp
// Good
public SchemaExportResult Export(SchemaExportRequest request)
{
    // Map DTO → Domain
    var schema = await _extractor.ExtractSchemaAsync(request.ConnectionString);
    
    // Map Domain → DTO
    return new SchemaExportResult { Success = true, ... };
}
```

### Security

- **Never trust user input**: Validate and sanitize all inputs
- **Use parameterized queries**: Always use parameters for SQL queries
- **Validate paths**: Use `PathValidator` for all file path operations
- **No secrets in code**: Never commit connection strings, passwords, or API keys

### Logging

- Use Serilog for structured logging
- Log at appropriate levels:
  - `Verbose` - Detailed diagnostic information
  - `Debug` - Debugging information
  - `Information` - General informational messages
  - `Warning` - Warnings about potential issues
  - `Error` - Error messages for recoverable errors
  - `Fatal` - Critical errors that require immediate attention

```csharp
_logger.LogInformation("Exporting schema from database {DatabaseName}", databaseName);
_logger.LogError(ex, "Failed to extract schema from {ConnectionString}", connectionString);
```

## Testing

### Test Requirements

- Write unit tests for all new functionality
- Use XUnit for testing
- Aim for high test coverage
- Test edge cases and error conditions

### Test Organization

```
DbDiff.Application.Tests/
├── Services/
│   └── SchemaExportServiceTests.cs
├── Validation/
│   └── PathValidatorTests.cs
└── Formatters/
    └── CustomTextFormatterTests.cs
```

### Test Naming Convention

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = "test";
    
    // Act
    var result = Method(input);
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~PathValidatorTests"
```

## Submitting Changes

### Pull Request Process

1. **Update documentation** if you've changed functionality
2. **Update CHANGELOG.md** following [Keep a Changelog](https://keepachangelog.com/) format
3. **Ensure all tests pass** locally
4. **Create a pull request** with a clear title and description
5. **Link related issues** in the PR description
6. **Respond to review feedback** promptly

### PR Checklist

Before submitting, ensure:

- [ ] Code follows the project's coding standards
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] No linter warnings or errors
- [ ] Hexagonal architecture principles followed
- [ ] Proper layer separation maintained
- [ ] Security best practices followed

### Commit Messages

Write clear, descriptive commit messages:

```
Add PostgreSQL schema extractor

- Implement ISchemaExtractor for PostgreSQL
- Add connection string validation
- Add tests for PostgreSQL extractor
- Update documentation

Fixes #123
```

### Branch Naming

- `feature/description` - New features
- `fix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring
- `test/description` - Test improvements

## Release Process

Releases are handled by maintainers:

1. Update `CHANGELOG.md` with release version and date
2. Create a version tag following semver: `git tag v1.0.0`
3. Push the tag: `git push origin v1.0.0`
4. GitHub Actions automatically builds and publishes the release

### Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

## Getting Help

- **Questions**: Open a discussion on GitHub
- **Bugs**: Open an issue with the bug report template
- **Features**: Open an issue with the feature request template
- **Security**: See [SECURITY.md](SECURITY.md) for reporting security issues

## Recognition

Contributors will be recognized in release notes and the repository's contributor list.

Thank you for contributing to DbDiff! 🎉

