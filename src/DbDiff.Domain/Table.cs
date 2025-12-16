namespace DbDiff.Domain;

public class Table
{
    public string SchemaName { get; init; }
    public string TableName { get; init; }
    public IReadOnlyList<Column> Columns { get; init; }

    public Table(string schemaName, string tableName, IEnumerable<Column> columns)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name cannot be null or empty.", nameof(schemaName));
        
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

        SchemaName = schemaName;
        TableName = tableName;
        Columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
    }

    public string FullName => $"{SchemaName}.{TableName}";
}

