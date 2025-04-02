namespace MPR.CodeGenTool.Models;

public class EntityMetadata
{
    public string ContextName { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public Type ClrType { get; set; } = default!;
    public List<KeyProperty> PrimaryKeyProperties { get; set; } = new();
}

public class KeyProperty
{
    public string Name { get; set; } = default!;
    public Type Type { get; set; } = default!;
    public bool IsNullable { get; set; }
}