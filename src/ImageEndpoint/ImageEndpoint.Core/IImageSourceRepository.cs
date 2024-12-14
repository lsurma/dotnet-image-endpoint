namespace ImageEndpoint.Core;

public interface IImageSourceRepository
{
    Task<Stream> GetFileContentByIdAsync(string fileId);
    
    Task<bool> FileExistsAsync(string fileId);
}