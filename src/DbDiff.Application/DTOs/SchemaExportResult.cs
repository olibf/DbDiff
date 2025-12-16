namespace DbDiff.Application.DTOs;

public class SchemaExportResult
{
    public bool Success { get; init; }
    public string ExportedFilePath { get; init; }
    public int ObjectCount { get; init; }
    public string? ErrorMessage { get; init; }

    private SchemaExportResult(bool success, string exportedFilePath, int objectCount, string? errorMessage)
    {
        Success = success;
        ExportedFilePath = exportedFilePath;
        ObjectCount = objectCount;
        ErrorMessage = errorMessage;
    }

    public static SchemaExportResult SuccessResult(string exportedFilePath, int objectCount)
    {
        return new SchemaExportResult(true, exportedFilePath, objectCount, null);
    }

    public static SchemaExportResult FailureResult(string errorMessage)
    {
        return new SchemaExportResult(false, string.Empty, 0, errorMessage);
    }
}

