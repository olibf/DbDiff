using DbDiff.Application.DTOs;
using DbDiff.Application.Formatters;

using Microsoft.Extensions.Logging;

namespace DbDiff.Application.Services;

public class SchemaExportService
{
    private readonly ISchemaFormatter _schemaFormatter;
    private readonly ILogger<SchemaExportService> _logger;
    private readonly Func<DatabaseType, ISchemaExtractor> _extractorFactory;

    public SchemaExportService(
        Func<DatabaseType, ISchemaExtractor> extractorFactory,
        ISchemaFormatter schemaFormatter,
        ILogger<SchemaExportService> logger)
    {
        _extractorFactory = extractorFactory ?? throw new ArgumentNullException(nameof(extractorFactory));
        _schemaFormatter = schemaFormatter ?? throw new ArgumentNullException(nameof(schemaFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SchemaExportResult> ExportSchemaAsync(
        SchemaExportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            _logger.LogInformation("Starting schema extraction from {DatabaseType} database", request.DatabaseType);

            // Get the appropriate extractor for the database type
            var schemaExtractor = _extractorFactory(request.DatabaseType);

            // Extract schema from database
            var schema = await schemaExtractor.ExtractSchemaAsync(
                request.ConnectionString,
                cancellationToken);

            _logger.LogInformation("Successfully extracted schema from database {DatabaseName} with {TableCount} tables and {ViewCount} views",
                schema.DatabaseName,
                schema.Tables.Count,
                schema.Views.Count);

            // Format schema to text
            var formattedSchema = _schemaFormatter.Format(schema);

            // Ensure output directory exists
            var outputDirectory = Path.GetDirectoryName(request.OutputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                _logger.LogDebug("Created output directory: {OutputDirectory}", outputDirectory);
            }

            // Write to file
            await File.WriteAllTextAsync(request.OutputPath, formattedSchema, cancellationToken);

            _logger.LogInformation("Schema exported successfully to {OutputPath}", request.OutputPath);

            return SchemaExportResult.SuccessResult(request.OutputPath, schema.Tables.Count, schema.Views.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export schema");
            return SchemaExportResult.FailureResult(ex.Message);
        }
    }
}

