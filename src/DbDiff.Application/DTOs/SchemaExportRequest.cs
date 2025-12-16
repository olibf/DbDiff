namespace DbDiff.Application.DTOs;

public class SchemaExportRequest
{
    public string ConnectionString { get; init; }
    public string OutputPath { get; init; }

    public SchemaExportRequest(string connectionString, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

        ConnectionString = connectionString;
        OutputPath = outputPath;
    }
}

