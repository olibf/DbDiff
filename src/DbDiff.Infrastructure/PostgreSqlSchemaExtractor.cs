using Npgsql;

namespace DbDiff.Infrastructure;

public class PostgreSqlSchemaExtractor : ISchemaExtractor
{
    public async Task<DatabaseSchema> ExtractSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var databaseName = connection.Database;
        var extractedAt = DateTime.UtcNow;

        var tables = await ExtractTablesAsync(connection, cancellationToken);

        return new DatabaseSchema(databaseName, extractedAt, tables);
    }

    private static async Task<List<Table>> ExtractTablesAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        var tables = new List<Table>();

        // Query to get all user tables (excluding system schemas)
        const string tableQuery = @"
            SELECT 
                table_schema,
                table_name
            FROM information_schema.tables
            WHERE table_type = 'BASE TABLE'
                AND table_schema NOT IN ('pg_catalog', 'information_schema')
            ORDER BY table_schema, table_name";

        await using var tableCommand = new NpgsqlCommand(tableQuery, connection);
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

    private static async Task<List<Column>> ExtractColumnsAsync(
        NpgsqlConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var columns = new List<Column>();

        const string columnQuery = @"
            SELECT 
                c.column_name,
                c.data_type,
                c.is_nullable,
                c.character_maximum_length,
                c.numeric_precision,
                c.numeric_scale,
                c.ordinal_position
            FROM information_schema.columns c
            WHERE c.table_schema = @SchemaName 
                AND c.table_name = @TableName
            ORDER BY c.ordinal_position";

        await using var columnCommand = new NpgsqlCommand(columnQuery, connection);
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

