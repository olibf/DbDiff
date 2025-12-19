using System.Text;

namespace DbDiff.Application.Formatters;

public class CustomTextFormatter : ISchemaFormatter
{
    public bool IncludeOrdinalPosition { get; set; } = true;
    public bool IncludeViewDefinitions { get; set; } = true;

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
            FormatColumns(sb, table.Columns);
            sb.AppendLine();
        }

        // Sort views alphabetically by full name for deterministic output
        var sortedViews = schema.Views
            .OrderBy(v => v.SchemaName)
            .ThenBy(v => v.ViewName)
            .ToList();

        foreach (var view in sortedViews)
        {
            sb.AppendLine($"VIEW: {view.FullName}");
            
            // Include view definition if available and enabled
            if (IncludeViewDefinitions && !string.IsNullOrWhiteSpace(view.Definition))
            {
                sb.AppendLine("  DEFINITION:");
                // Indent each line of the definition
                var definitionLines = view.Definition.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in definitionLines)
                {
                    sb.AppendLine($"    {line}");
                }
            }
            
            FormatColumns(sb, view.Columns);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void FormatColumns(StringBuilder sb, IReadOnlyList<Column> columns)
    {
        // Sort columns alphabetically by name
        var sortedColumns = columns
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
    }
}

