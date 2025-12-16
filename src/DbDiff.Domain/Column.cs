namespace DbDiff.Domain;

public class Column
{
    public string Name { get; init; }
    public DataType DataType { get; init; }
    public bool IsNullable { get; init; }
    public int? MaxLength { get; init; }
    public int? Precision { get; init; }
    public int? Scale { get; init; }
    public int OrdinalPosition { get; init; }

    public Column(
        string name, 
        DataType dataType, 
        bool isNullable, 
        int ordinalPosition,
        int? maxLength = null, 
        int? precision = null, 
        int? scale = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name cannot be null or empty.", nameof(name));

        Name = name;
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        IsNullable = isNullable;
        OrdinalPosition = ordinalPosition;
        MaxLength = maxLength;
        Precision = precision;
        Scale = scale;
    }
}

