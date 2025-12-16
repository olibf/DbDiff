namespace DbDiff.Domain;

public class DatabaseSchema
{
    public string DatabaseName { get; init; }
    public DateTime ExtractedAt { get; init; }
    public IReadOnlyList<Table> Tables { get; init; }

    public DatabaseSchema(string databaseName, DateTime extractedAt, IEnumerable<Table> tables)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Database name cannot be null or empty.", nameof(databaseName));

        DatabaseName = databaseName;
        ExtractedAt = extractedAt;
        Tables = tables?.ToList() ?? throw new ArgumentNullException(nameof(tables));
    }
}

