namespace DbDiff.Application.DTOs;

public class SchemaExportResult
{
    public bool Success { get; init; }
    public string ExportedFilePath { get; init; }
    public int TableCount { get; init; }
    public int ViewCount { get; init; }
    public int ObjectCount => TableCount + ViewCount;
    public string? ErrorMessage { get; init; }

    private SchemaExportResult(bool success, string exportedFilePath, int tableCount, int viewCount, string? errorMessage)
    {
        Success = success;
        ExportedFilePath = exportedFilePath;
        TableCount = tableCount;
        ViewCount = viewCount;
        ErrorMessage = errorMessage;
    }

    public static SchemaExportResult SuccessResult(string exportedFilePath, int tableCount, int viewCount)
    {
        return new SchemaExportResult(true, exportedFilePath, tableCount, viewCount, null);
    }

    public static SchemaExportResult FailureResult(string errorMessage)
    {
        return new SchemaExportResult(false, string.Empty, 0, 0, errorMessage);
    }
}

