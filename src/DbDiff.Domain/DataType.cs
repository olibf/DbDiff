namespace DbDiff.Domain;

public class DataType
{
    public string TypeName { get; init; }

    public DataType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

        TypeName = typeName.ToLowerInvariant();
    }

    public override string ToString() => TypeName;

    public override bool Equals(object? obj)
    {
        if (obj is DataType other)
            return TypeName.Equals(other.TypeName, StringComparison.OrdinalIgnoreCase);

        return false;
    }

    public override int GetHashCode() => TypeName.GetHashCode(StringComparison.OrdinalIgnoreCase);
}

