using Microsoft.Data.SqlClient;

namespace DbDiff.Infrastructure;

public class MsSqlSchemaExtractor : ISchemaExtractor
{
    public async Task<DatabaseSchema> ExtractSchemaAsync(
        string connectionString, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var databaseName = connection.Database;
        var extractedAt = DateTime.UtcNow;

        var tables = await ExtractTablesAsync(connection, cancellationToken);
        var views = await ExtractViewsAsync(connection, cancellationToken);

        return new DatabaseSchema(databaseName, extractedAt, tables, views);
    }

    private async Task<List<Table>> ExtractTablesAsync(
        SqlConnection connection, 
        CancellationToken cancellationToken)
    {
        var tables = new List<Table>();

        // Query to get all user tables
        const string tableQuery = @"
            SELECT 
                TABLE_SCHEMA,
                TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        await using var tableCommand = new SqlCommand(tableQuery, connection);
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

    private async Task<List<View>> ExtractViewsAsync(
        SqlConnection connection, 
        CancellationToken cancellationToken)
    {
        var views = new List<View>();

        // Query to get all user views with their definitions (excluding system views)
        // Using sys.views and sys.sql_modules to get the full definition (no 4000 char limit)
        const string viewQuery = @"
            SELECT 
                s.name AS SchemaName,
                v.name AS ViewName,
                m.definition AS Definition
            FROM sys.views v
            INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
            LEFT JOIN sys.sql_modules m ON v.object_id = m.object_id
            WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA')
            ORDER BY s.name, v.name";

        await using var viewCommand = new SqlCommand(viewQuery, connection);
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

    private async Task<List<Column>> ExtractColumnsAsync(
        SqlConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var columns = new List<Column>();

        const string columnQuery = @"
            SELECT 
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                c.ORDINAL_POSITION
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_SCHEMA = @SchemaName 
                AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION";

        await using var columnCommand = new SqlCommand(columnQuery, connection);
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
            int? precision = columnReader.IsDBNull(4) ? null : Convert.ToInt32(columnReader.GetByte(4));
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

