namespace DbDiff.Infrastructure;

public class SchemaExtractorFactory
{
    public static ISchemaExtractor CreateExtractor(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => new MsSqlSchemaExtractor(),
            DatabaseType.PostgreSql => new PostgreSqlSchemaExtractor(),
            DatabaseType.Sqlite => new SqliteSqlSchemaExtractor(),
            _ => throw new ArgumentException($"Unsupported database type: {databaseType}", nameof(databaseType))
        };
    }
}

