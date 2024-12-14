using ImageEndpoint.Core;

namespace ImageEndpoint.Host.Razor;

public class FileImageSourceRepository : IImageSourceRepository
{
    private readonly string _baseDirectory;

    public FileImageSourceRepository(IWebHostEnvironment env)
    {
        _baseDirectory = Path.Combine(env.WebRootPath, "blobs/src");
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
    
    public Task<bool> FileExistsAsync(string fileId)
    {
        var filePath = Path.Combine(_baseDirectory, $"{fileId}"); // Assuming files are stored with .txt extension
        return Task.FromResult(File.Exists(filePath));
    }
}