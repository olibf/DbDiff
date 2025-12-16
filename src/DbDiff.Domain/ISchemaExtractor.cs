namespace DbDiff.Domain;

public interface ISchemaExtractor
{
    Task<DatabaseSchema> ExtractSchemaAsync(string connectionString, CancellationToken cancellationToken = default);
}

