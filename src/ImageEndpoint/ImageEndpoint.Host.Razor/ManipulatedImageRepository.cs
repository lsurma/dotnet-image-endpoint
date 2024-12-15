using ImageEndpoint.Core;
using FileInfo=ImageEndpoint.Core.FileInfo;

namespace ImageEndpoint.Host.Razor;

public class FileConvertedImagesRepository : ImageRepositoryBase, IConvertedImagesRepository
{
    private readonly string _baseDirectory;

    public FileConvertedImagesRepository(IWebHostEnvironment env)
    {
        _baseDirectory = Path.Combine(env.WebRootPath, "blobs/converted");
    }
    
    public Task<Stream> GetFileContentAsync(ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, GetFilePath(args));
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        return Task.FromResult<Stream>(File.OpenRead(filePath));        
    }
    
    public Task<bool> SaveContentAsync(Stream fileStream, ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        EnsureDirectoryExists(_baseDirectory);
        
        var filePath = Path.Combine(_baseDirectory, GetFilePath(args));
        
        using var file = File.Create(filePath);
        fileStream.CopyTo(file);
        
        return Task.FromResult(true);
    }
    
    public Task<bool> DeleteAsync(ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, GetFilePath(args));
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }
    
    public Task<bool> ExistsAsync(ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, GetFilePath(args));
        return Task.FromResult(File.Exists(filePath));
    }
    
    private void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
    
    
    public Task<FileInfo> GetFileInfoAsync(ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, GetFilePath(args));
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var fileInfo = new FileInfo(filePath);
        return Task.FromResult(fileInfo);
    }

}