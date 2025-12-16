namespace DbDiff.Application.Formatters;

public interface ISchemaFormatter
{
    string Format(DatabaseSchema schema);
}

