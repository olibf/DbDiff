using Microsoft.Data.Sqlite;

namespace DbDiff.Infrastructure;

public class SqliteSqlSchemaExtractor : ISchemaExtractor
{
    public async Task<DatabaseSchema> ExtractSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var databaseName = connection.Database;
        var extractedAt = DateTime.UtcNow;

        var tables = await ExtractTablesAsync(connection, cancellationToken);
        var views = await ExtractViewsAsync(connection, cancellationToken);

        return new DatabaseSchema(databaseName, extractedAt, tables, views);
    }

    private static async Task<List<Table>> ExtractTablesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var tables = new List<Table>();

        // Query to get all user tables (excluding system schemas)
        const string tableQuery = @"
            SELECT
                'sqlite' as table_schema,
                name     as table_name
            FROM sqlite_master
            WHERE type = 'table'
                AND name NOT LIKE 'sqlite_%'
            ORDER BY name";

        await using var tableCommand = new SqliteCommand(tableQuery, connection);
        await using var tableReader = await tableCommand.ExecuteReaderAsync(cancellationToken);

        var tableInfoList = new List<(string Schema, string Name)>();
        while (await tableReader.ReadAsync(cancellationToken))
        {
            var schemaName = tableReader.GetString(0);
            var tableName = tableReader.GetString(1);
            tableInfoList.Add((schemaName, tableName));
        }

        await tableReader.CloseAsync();

        // Extract columns for each table
        foreach (var (schemaName, tableName) in tableInfoList)
        {
            var columns = await ExtractColumnsAsync(connection, schemaName, tableName, cancellationToken);
            tables.Add(new Table(schemaName, tableName, columns));
        }

        return tables;
    }

    private static async Task<List<View>> ExtractViewsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var views = new List<View>();

        // Query to get all user views with their definitions (excluding system schemas)
        const string viewQuery = @"
            SELECT
                'sqlite' as table_schema,
                name     as table_name,
                sql      as definition
            FROM sqlite_master
            WHERE type = 'view'
                AND name NOT LIKE 'sqlite_%'
            ORDER BY name";

        await using var viewCommand = new SqliteCommand(viewQuery, connection);
        await using var viewReader = await viewCommand.ExecuteReaderAsync(cancellationToken);

        var viewInfoList = new List<(string Schema, string Name, string? Definition)>();
        while (await viewReader.ReadAsync(cancellationToken))
        {
            var schemaName = viewReader.GetString(0);
            var viewName = viewReader.GetString(1);
            var definition = viewReader.IsDBNull(2) ? null : viewReader.GetString(2);
            viewInfoList.Add((schemaName, viewName, definition));
        }

        await viewReader.CloseAsync();

        // Extract columns for each view
        foreach (var (schemaName, viewName, definition) in viewInfoList)
        {
            var columns = await ExtractColumnsAsync(connection, schemaName, viewName, cancellationToken);
            views.Add(new View(schemaName, viewName, columns, definition));
        }

        return views;
    }

    private static async Task<List<Column>> ExtractColumnsAsync(
        SqliteConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var columns = new List<Column>();

        const string columnQuery = @"
            SELECT
                name        as column_name,
                type        as data_type,
                ""notnull"" as is_nullable,
                null        as character_maximum_length,
                null        as numeric_precision,
                null        as numeric_scale,
                cid         as ordinal_position
            FROM pragma_table_info(@TableName) c
            ORDER BY cid";

        await using var columnCommand = new SqliteCommand(columnQuery, connection);
        columnCommand.Parameters.AddWithValue("@SchemaName", schemaName);
        columnCommand.Parameters.AddWithValue("@TableName", tableName);

        await using var columnReader = await columnCommand.ExecuteReaderAsync(cancellationToken);

        while (await columnReader.ReadAsync(cancellationToken))
        {
            var columnName = columnReader.GetString(0);
            var dataTypeName = columnReader.GetString(1);
            var isNullableStr = columnReader.GetString(2);
            var isNullable = isNullableStr.Equals("YES", StringComparison.OrdinalIgnoreCase);

            int? maxLength = columnReader.IsDBNull(3) ? null : columnReader.GetInt32(3);
            int? precision = columnReader.IsDBNull(4) ? null : columnReader.GetInt32(4);
            int? scale = columnReader.IsDBNull(5) ? null : columnReader.GetInt32(5);
            var ordinalPosition = columnReader.GetInt32(6);

            var dataType = new DataType(dataTypeName);
            var column = new Column(
                columnName,
                dataType,
                isNullable,
                ordinalPosition,
                maxLength,
                precision,
                scale);

            columns.Add(column);
        }

        return columns;
    }
}

