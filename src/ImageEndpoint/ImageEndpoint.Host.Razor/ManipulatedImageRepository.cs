using ImageEndpoint.Core;

namespace ImageEndpoint.Host.Razor;

public class ManipulatedImageRepository : IManipulatedImageRepository
{
    private readonly string _baseDirectory;

    public ManipulatedImageRepository(IWebHostEnvironment env)
    {
        _baseDirectory = Path.Combine(env.WebRootPath, "blobs/converted");
    }
    
    public async Task<Stream> GetFileContentByIdAsync(string fileId)
    {
        var filePath = Path.Combine(_baseDirectory, $"{fileId}"); // Assuming files are stored with .txt extension
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        return File.OpenRead(filePath);
    }
    
    public async Task<bool> SaveFileContentAsync(Stream fileStream, string fileId)
    {
        var filePath = Path.Combine(_baseDirectory, $"{fileId}"); // Assuming files are stored with .txt extension
        EnsureDirectoryExists(Path.GetDirectoryName(filePath));
        
        await using var file = File.Create(filePath);
        await fileStream.CopyToAsync(file);
        
        return true;
    }
    
    public async Task DeleteFileAsync(string fileId)
    {
        var filePath = Path.Combine(_baseDirectory, $"{fileId}"); // Assuming files are stored with .txt extension
        File.Delete(filePath);
    }
    
    public async Task<bool> FileExistsAsync(string fileId)
    {
        var filePath = Path.Combine(_baseDirectory, $"{fileId}"); // Assuming files are stored with .txt extension
        return File.Exists(filePath);
    }
    
    private void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}