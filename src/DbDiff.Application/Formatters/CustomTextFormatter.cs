using System.Text;

namespace DbDiff.Application.Formatters;

public class CustomTextFormatter : ISchemaFormatter
{
    public bool IncludeOrdinalPosition { get; set; } = true;

    public string Format(DatabaseSchema schema)
    {
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));

        var sb = new StringBuilder();

        // Header section
        sb.AppendLine($"DATABASE: {schema.DatabaseName}");
        sb.AppendLine($"EXTRACTED: {schema.ExtractedAt:yyyy-MM-ddTHH:mm:ss.fffZ}");
        sb.AppendLine();

        // Sort tables alphabetically by full name for deterministic output
        var sortedTables = schema.Tables
            .OrderBy(t => t.SchemaName)
            .ThenBy(t => t.TableName)
            .ToList();

        foreach (var table in sortedTables)
        {
            sb.AppendLine($"TABLE: {table.FullName}");

            // Sort columns alphabetically by name
            var sortedColumns = table.Columns
                .OrderBy(c => c.Name)
                .ToList();

            foreach (var column in sortedColumns)
            {
                sb.AppendLine($"  COLUMN: {column.Name}");
                
                if (IncludeOrdinalPosition)
                    sb.AppendLine($"    OrdinalPosition: {column.OrdinalPosition}");
                
                sb.AppendLine($"    Type: {column.DataType}");
                sb.AppendLine($"    Nullable: {(column.IsNullable ? "Yes" : "No")}");

                if (column.MaxLength.HasValue)
                    sb.AppendLine($"    MaxLength: {column.MaxLength.Value}");

                if (column.Precision.HasValue)
                    sb.AppendLine($"    Precision: {column.Precision.Value}");

                if (column.Scale.HasValue)
                    sb.AppendLine($"    Scale: {column.Scale.Value}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}

