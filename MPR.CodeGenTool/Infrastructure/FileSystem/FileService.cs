// Infrastructure/FileSystem/FileService.cs
using System.IO;
using System.Threading.Tasks;

namespace MPR.CodeGenTool.Infrastructure.FileSystem
{
    public class FileService
    {
        public async Task CreateDirectoryAsync(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            await Task.CompletedTask;
        }
        
        public async Task<string> ReadAllTextAsync(string path)
        {
            return await File.ReadAllTextAsync(path);
        }
        
        public async Task WriteAllTextAsync(string path, string content)
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await File.WriteAllTextAsync(path, content);
        }
        
        public async Task<bool> FileExistsAsync(string path)
        {
            return await Task.FromResult(File.Exists(path));
        }
        
        public async Task<bool> DirectoryExistsAsync(string path)
        {
            return await Task.FromResult(Directory.Exists(path));
        }
    }
}