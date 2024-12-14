namespace ImageEndpoint.Core;

public interface IManipulatedImageRepository
{
    Task<Stream> GetFileContentByIdAsync(string fileId);
    
    Task<bool> SaveFileContentAsync(Stream fileStream, string fileId);
    
    Task DeleteFileAsync(string fileId);
    
    Task<bool> FileExistsAsync(string fileId);
}