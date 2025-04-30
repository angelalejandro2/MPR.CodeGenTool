namespace MPR.CodeGenTool.Domain.Configuration
{
    /// <summary>
    /// Configuration for HTTP methods in controllers
    /// </summary>
    public class MethodsConfig
    {
        public bool Get { get; set; } = true;
        public bool GetById { get; set; } = true;
        public bool Post { get; set; } = true;
        public bool Put { get; set; } = true;
        public bool Delete { get; set; } = true;
    }
}