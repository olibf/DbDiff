using System.Reflection;

using DbDiff.Application.DTOs;
using DbDiff.Application.Formatters;
using DbDiff.Application.Services;
using DbDiff.Application.Validation;
using DbDiff.Domain;
using DbDiff.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

// Get version from assembly
var version = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion ?? "Unknown";

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "DBDIFF_")
    .Build();

// Setup Serilog with path validation
var logPath = configuration["Serilog:LogPath"] ?? "logs/dbdiff-.txt";
try
{
    logPath = PathValidator.ValidateLogPath(logPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: Invalid log path '{logPath}': {ex.Message}");
    return 1;
}

var serilogLogger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = serilogLogger;

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog(serilogLogger, dispose: true);
});

// Register application services
services.AddSingleton<ISchemaFormatter, CustomTextFormatter>();

// Register factory function for schema extractors
services.AddSingleton<Func<DatabaseType, ISchemaExtractor>>(serviceProvider =>
{
    var factory = new SchemaExtractorFactory();
    return (databaseType) => SchemaExtractorFactory.CreateExtractor(databaseType);
});

services.AddSingleton<SchemaExportService>();

var serviceProvider = services.BuildServiceProvider();

// Parse command-line arguments
string? connection = null;
string? output = null;
string? configFile = null;
string? databaseTypeArg = null;
bool showHelp = false;
bool ignorePosition = false;
bool excludeViewDefinitions = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--connection" or "-c":
            if (i + 1 < args.Length)
                connection = args[++i];
            break;
        case "--output" or "-o":
            if (i + 1 < args.Length)
                output = args[++i];
            break;
        case "--config":
            if (i + 1 < args.Length)
                configFile = args[++i];
            break;
        case "--database-type" or "-d":
            if (i + 1 < args.Length)
                databaseTypeArg = args[++i];
            break;
        case "--ignore-position":
            ignorePosition = true;
            break;
        case "--exclude-view-definitions":
            excludeViewDefinitions = true;
            break;
        case "--help" or "-h" or "-?":
            showHelp = true;
            break;
    }
}

if (showHelp || args.Length == 0)
{
    Console.WriteLine($"DbDiff - Database Schema Comparison Tool v{version}");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dbdiff --connection <connection-string> [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -c, --connection <string>    Database connection string (required)");
    Console.WriteLine("  -o, --output <path>          Output file path (default: schema.txt)");
    Console.WriteLine("  -d, --database-type <type>   Database type: sqlserver, postgresql (auto-detected if not specified)");
    Console.WriteLine("  --config <path>              Configuration file path");
    Console.WriteLine("  --ignore-position            Exclude column ordinal positions from output");
    Console.WriteLine("  --exclude-view-definitions   Exclude view SQL definitions from output");
    Console.WriteLine("  -h, --help                   Show help information");
    Console.WriteLine();
    Console.WriteLine("Configuration:");
    Console.WriteLine("  Connection strings can also be configured via:");
    Console.WriteLine("  - Environment variable: DBDIFF_ConnectionStrings__Default");
    Console.WriteLine("  - appsettings.json file");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  # SQL Server");
    Console.WriteLine("  dbdiff --connection \"Server=localhost;Database=MyDb;Trusted_Connection=true;\" --output schema.txt");
    Console.WriteLine();
    Console.WriteLine("  # PostgreSQL");
    Console.WriteLine("  dbdiff --connection \"Host=localhost;Database=mydb;Username=user;Password=pass\" --database-type postgresql");
    Console.WriteLine();
    Console.WriteLine("  # Auto-detect database type and ignore column positions");
    Console.WriteLine("  dbdiff --connection \"Host=localhost;Database=mydb;Username=user;Password=pass\" --ignore-position");
    return 0;
}

try
{
    // Load custom config if specified (with path validation)
    if (!string.IsNullOrWhiteSpace(configFile))
    {
        try
        {
            // Validate config file path - restrict to current directory and subdirectories for security
            var validatedConfigPath = PathValidator.ValidateConfigPath(
                configFile,
                allowedBasePath: Directory.GetCurrentDirectory());

            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(validatedConfigPath, optional: false, reloadOnChange: false)
                .AddEnvironmentVariables(prefix: "DBDIFF_")
                .Build();
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Error: Invalid configuration file: {ex.Message}");
            return 1;
        }
    }

    // Priority: CLI args > Environment variables > Config file
    var connectionString = connection
        ?? Environment.GetEnvironmentVariable("DBDIFF_ConnectionStrings__Default")
        ?? configuration["ConnectionStrings:Default"];

    var outputPath = output
        ?? Environment.GetEnvironmentVariable("DBDIFF_Export__OutputPath")
        ?? configuration["Export:OutputPath"]
        ?? "schema.txt";

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.Error.WriteLine("Error: Connection string is required.");
        Console.Error.WriteLine("Provide it via:");
        Console.Error.WriteLine("  --connection argument");
        Console.Error.WriteLine("  DBDIFF_ConnectionStrings__Default environment variable");
        Console.Error.WriteLine("  appsettings.json configuration file");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Run 'dbdiff --help' for more information.");
        return 1;
    }

    // Determine database type
    DatabaseType databaseType;
    if (!string.IsNullOrWhiteSpace(databaseTypeArg))
    {
        // Explicit database type specified
        if (!Enum.TryParse<DatabaseType>(databaseTypeArg, ignoreCase: true, out databaseType))
        {
            Console.Error.WriteLine($"Error: Invalid database type '{databaseTypeArg}'.");
            Console.Error.WriteLine("Valid values: sqlserver, postgresql");
            return 1;
        }
    }
    else
    {
        // Auto-detect database type from connection string
        databaseType = DetectDatabaseType(connectionString);
        Console.WriteLine($"Auto-detected database type: {databaseType}");
    }

    Console.WriteLine($"DbDiff - Database Schema Export v{version}");
    Console.WriteLine($"Database Type: {databaseType}");
    Console.WriteLine($"Output: {outputPath}");
    Console.WriteLine();

    // Configure formatter based on command-line options
    var formatter = serviceProvider.GetRequiredService<ISchemaFormatter>();
    if (formatter is CustomTextFormatter customFormatter)
    {
        customFormatter.IncludeOrdinalPosition = !ignorePosition;
        customFormatter.IncludeViewDefinitions = !excludeViewDefinitions;
    }

    var exportService = serviceProvider.GetRequiredService<SchemaExportService>();

    // Note: Path validation happens in SchemaExportRequest constructor
    SchemaExportRequest request;
    try
    {
        request = new SchemaExportRequest(connectionString, outputPath, databaseType);
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.Error.WriteLine($"✗ Security Error: {ex.Message}");
        Log.Error(ex, "Unauthorized path access attempt");
        return 1;
    }

    var result = await exportService.ExportSchemaAsync(request);

    if (result.Success)
    {
        Console.WriteLine($"✓ Successfully exported schema to: {result.ExportedFilePath}");
        Console.WriteLine($"  Tables exported: {result.ObjectCount}");
        return 0;
    }
    else
    {
        Console.Error.WriteLine($"✗ Export failed: {result.ErrorMessage}");
        return 1;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"✗ Unexpected error: {ex.Message}");
    Log.Error(ex, "Unexpected error during export");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static DatabaseType DetectDatabaseType(string connectionString)
{
    // Check for PostgreSQL keywords
    if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase))
    {
        return DatabaseType.PostgreSql;
    }

    // Check for SQL Server keywords
    if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Integrated Security=", StringComparison.OrdinalIgnoreCase))
    {
        return DatabaseType.SqlServer;
    }

    // Default to SQL Server for backward compatibility
    Console.WriteLine("Warning: Could not auto-detect database type. Defaulting to SQL Server.");
    return DatabaseType.SqlServer;
}
