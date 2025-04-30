namespace MPR.CodeGenTool.Domain.Configuration
{
    /// <summary>
    /// Configuration for a DbContext
    /// </summary>
    public class DbContextConfig
    {
        public string Provider { get; set; } = "SqlServer";
        public string ConnectionStringName { get; set; }
    }
}