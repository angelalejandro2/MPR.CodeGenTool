namespace MPR.CodeGenTool.Models;
public class EntityModel
{
    public string Name { get; set; } = "";
    public List<PropertyModel> Properties { get; set; } = new();
    public List<string> PrimaryKeys { get; set; } = new();
}