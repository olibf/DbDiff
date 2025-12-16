using DbDiff.Application.Validation;

namespace DbDiff.Application.DTOs;

public class SchemaExportRequest
{
    public string ConnectionString { get; init; }
    public string OutputPath { get; init; }

    public SchemaExportRequest(string connectionString, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        
        // Validate and sanitize the output path to prevent path traversal attacks
        OutputPath = PathValidator.ValidateOutputPath(outputPath);
        ConnectionString = connectionString;
    }
}

