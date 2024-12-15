namespace ImageEndpoint.Core;

public interface ISourceImagesRepository
{
    Task<Stream> GetFileContentAsync(ImageConversionArgs args, CancellationToken cancellationToken = default);

    Task<FileInfo> GetFileInfoAsync(ImageConversionArgs args, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(ImageConversionArgs args, CancellationToken cancellationToken = default);
}