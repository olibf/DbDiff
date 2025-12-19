namespace DbDiff.Domain;

public class View
{
    public string SchemaName { get; init; }
    public string ViewName { get; init; }
    public string? Definition { get; init; }
    public IReadOnlyList<Column> Columns { get; init; }

    public View(string schemaName, string viewName, IEnumerable<Column> columns, string? definition = null)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name cannot be null or empty.", nameof(schemaName));
        
        if (string.IsNullOrWhiteSpace(viewName))
            throw new ArgumentException("View name cannot be null or empty.", nameof(viewName));

        SchemaName = schemaName;
        ViewName = viewName;
        Definition = definition;
        Columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
    }

    public string FullName => $"{SchemaName}.{ViewName}";
}

