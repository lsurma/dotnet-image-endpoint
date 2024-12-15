using ImageEndpoint.Core;
using FileInfo=ImageEndpoint.Core.FileInfo;

namespace ImageEndpoint.Host.Razor;

public class LocalDiskSourceImagesRepository : ImageRepositoryBase, ISourceImagesRepository
{
    private readonly string _baseDirectory;

    public LocalDiskSourceImagesRepository(IWebHostEnvironment env)
    {
        _baseDirectory = Path.Combine(env.WebRootPath, "blobs/src");
    }

    public async Task<Stream> GetFileContentAsync(ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, args.SourceImageId);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        return File.OpenRead(filePath);
    }
    
    public Task<FileInfo> GetFileInfoAsync(string sourceImageId, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, sourceImageId);
        var extension = Path.GetExtension(filePath);
        var fileInfo = new FileInfo(
            sourceImageId,
            ExtToFormat(extension),
            0,
            DateTimeOffset.Now
        );
        return Task.FromResult(fileInfo);
    }

    public Task<bool> ExistsAsync(ImageConversionArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseDirectory, args.SourceImageId);
        return Task.FromResult(File.Exists(filePath));
    }
}