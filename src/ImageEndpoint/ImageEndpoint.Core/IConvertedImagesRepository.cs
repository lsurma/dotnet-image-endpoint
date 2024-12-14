namespace ImageEndpoint.Core;

public interface IConvertedImagesRepository
{
    Task<Stream> GetFileContentAsync(ImageConversionArgs args, CancellationToken cancellationToken = default);
    
    Task<bool> SaveContentAsync(Stream fileStream, ImageConversionArgs args, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(ImageConversionArgs args, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(ImageConversionArgs args, CancellationToken cancellationToken = default);
}